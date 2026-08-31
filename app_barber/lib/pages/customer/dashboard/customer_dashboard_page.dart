import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:hive/hive.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/barbershop.dart';
import 'package:app_barber/models/appointment.dart';
import 'package:app_barber/repositories/barbershop_repository.dart';
import 'package:app_barber/repositories/appointment_repository.dart';
import 'package:app_barber/pages/customer/appointment/customer_booking_page.dart';
import 'package:app_barber/pages/customer/notification/customer_notifications_page.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:url_launcher/url_launcher.dart';

class CustomerDashboardPage extends StatefulWidget {
  const CustomerDashboardPage({super.key});

  @override
  State<CustomerDashboardPage> createState() => _CustomerDashboardPageState();
}

class _CustomerDashboardPageState extends State<CustomerDashboardPage> {
  late final BarbershopRepository _barbershopRepo;
  late final AppointmentRepository _appointmentRepo;
  
  Barbershop? _barbershop;
  List<Appointment> _appointments = [];
  bool _isLoading = true;
  Timer? _ticker;
  
  String _customerName = 'Cliente';
  String _customerPhoto = '';

  @override
  void initState() {
    super.initState();
    final apiClient = ApiClient();
    _barbershopRepo = BarbershopRepository(apiClient);
    _appointmentRepo = AppointmentRepository(apiClient);
    _loadData();
    _ticker = Timer.periodic(const Duration(seconds: 1), (_) {
      if (mounted && _hasInProgressAppointment()) {
        setState(() {});
      }
    });
  }

  @override
  void dispose() {
    _ticker?.cancel();
    super.dispose();
  }

  bool _hasInProgressAppointment() {
    return _appointments.any((a) => a.status == 4);
  }

  Future<void> _loadData() async {
    final authBox = Hive.box('auth');
    final barbershopId = authBox.get('barbershopId', defaultValue: '');
    final token = authBox.get('token', defaultValue: '');
    
    if (token.isNotEmpty) {
      try {
        final savedName = (authBox.get('name', defaultValue: '') as String).trim();
        if (savedName.isNotEmpty) {
          _customerName = savedName;
        } else {
          Map<String, dynamic> decodedToken = JwtDecoder.decode(token);
          _customerName = decodedToken['name'] ?? 
                          decodedToken['unique_name'] ?? 
                          decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? 
                          'Cliente';
        }
        _customerPhoto = authBox.get('photo', defaultValue: '');
        
        if (barbershopId.isNotEmpty) {
          final futures = await Future.wait([
            _barbershopRepo.getBarbershop(barbershopId),
            _appointmentRepo.getCustomerAppointments(barbershopId),
          ]);

          if (mounted) {
            setState(() {
              _barbershop = futures[0] as Barbershop?;
              _appointments = futures[1] as List<Appointment>;
              _isLoading = false;
            });
          }
        } else {
          if (mounted) setState(() => _isLoading = false);
        }
      } catch (e) {
        if (mounted) setState(() => _isLoading = false);
      }
    } else {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    return Scaffold(
      appBar: PreferredSize(
        preferredSize: const Size.fromHeight(70),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 8.0),
            child: _buildHeader(),
          ),
        ),
      ),
      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: _loadData,
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 12.0),
            physics: const AlwaysScrollableScrollPhysics(),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildWelcomeCard(),
                const SizedBox(height: 20),
                _buildLiveServiceCard(),
                const SizedBox(height: 20),
                _buildIndicatorsGrid(),
                const SizedBox(height: 80),
              ],
            ),
          ),
        ),
      ),
    );
  }

  ImageProvider? _getProfileImage() {
    final photo = _customerPhoto.trim();
    if (photo.isEmpty) return null;
    if (photo.startsWith('http')) return NetworkImage(photo);
    try {
      final base64Str = photo.contains(',') ? photo.split(',').last : photo;
      final clean = base64Str.replaceAll(RegExp(r'\s+'), '');
      return MemoryImage(base64Decode(clean));
    } catch (_) {
      return null;
    }
  }

  Widget _buildHeader() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        CircleAvatar(
          radius: 24,
          backgroundColor: Theme.of(context).dividerColor,
          backgroundImage: _getProfileImage(),
          child: _getProfileImage() == null ? Icon(Icons.person, color: Theme.of(context).iconTheme.color) : null,
        ),
        IconButton(
          icon: const Icon(Icons.notifications_none, size: 28),
          onPressed: () {
            Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const CustomerNotificationsPage()),
            );
          },
        ),
      ],
    );
  }

  Widget _buildWelcomeCard() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.primary,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Olá, ${_customerName.split(' ').first}!',
            style: const TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          Text(
            _barbershop != null ? 'Bem-vindo à ${_barbershop!.name}' : 'Bem-vindo(a)!',
            style: TextStyle(color: Colors.white.withOpacity(0.9), fontSize: 16),
          ),
        ],
      ),
    );
  }

  String _formatElapsedTime(DateTime? startedAt) {
    if (startedAt == null) return '00:00';
    final duration = DateTime.now().toUtc().difference(startedAt);
    final minutes = duration.inMinutes.abs();
    final seconds = (duration.inSeconds.abs() % 60);
    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}';
  }

  void _openWhatsApp(String phone) async {
    final clean = phone.replaceAll(RegExp(r'\D'), '');
    if (clean.isEmpty) return;
    final uri = Uri.parse('https://wa.me/55$clean');
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  void _showRatingModal(Appointment appt) {
    int selectedRating = 5;
    final commentCtrl = TextEditingController();
    bool isSubmitting = false;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setSheetState) => SafeArea(
          top: false,
          child: Padding(
            padding: EdgeInsets.only(
              left: 24, right: 24, top: 24,
              bottom: MediaQuery.of(ctx).viewInsets.bottom + 24,
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Container(
                  width: 40,
                  height: 4,
                  decoration: BoxDecoration(color: Colors.grey[400], borderRadius: BorderRadius.circular(2)),
                ),
                const SizedBox(height: 16),
                const Text(
                  'Como foi seu atendimento?',
                  style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                Text(
                  'Avalie seu serviço com ${appt.barberName}',
                  style: TextStyle(color: Colors.grey[600], fontSize: 14),
                ),
                const SizedBox(height: 20),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: List.generate(5, (index) {
                    final star = index + 1;
                    return IconButton(
                      iconSize: 38,
                      icon: Icon(
                        star <= selectedRating ? Icons.star : Icons.star_border,
                        color: Colors.amber,
                      ),
                      onPressed: () {
                        setSheetState(() => selectedRating = star);
                      },
                    );
                  }),
                ),
                const SizedBox(height: 16),
                TextField(
                  controller: commentCtrl,
                  maxLines: 3,
                  decoration: InputDecoration(
                    hintText: 'Deixe um comentário sobre o atendimento (opcional)',
                    filled: true,
                    fillColor: Theme.of(context).cardColor,
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                  ),
                ),
                const SizedBox(height: 20),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: isSubmitting
                        ? null
                        : () async {
                            setSheetState(() => isSubmitting = true);
                            final ok = await _appointmentRepo.rateAppointment(
                              appt.id,
                              selectedRating,
                              commentCtrl.text.trim(),
                            );
                            if (mounted) {
                              Navigator.pop(ctx);
                              if (ok) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(content: Text('Avaliação enviada com sucesso! ⭐'), backgroundColor: Colors.green),
                                );
                                _loadData();
                              }
                            }
                          },
                    style: ElevatedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    child: isSubmitting
                        ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                        : const Text('Enviar Avaliação', style: TextStyle(fontWeight: FontWeight.bold)),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildLiveServiceCard() {
    final inProgressAppt = _appointments.cast<Appointment?>().firstWhere(
      (a) => a?.status == 4,
      orElse: () => null,
    );

    if (inProgressAppt != null) {
      return Container(
        width: double.infinity,
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: Theme.of(context).cardColor,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: Colors.purpleAccent, width: 2),
          boxShadow: [
            BoxShadow(
              color: Colors.purpleAccent.withOpacity(0.2),
              blurRadius: 16,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                  decoration: BoxDecoration(
                    color: Colors.purpleAccent.withOpacity(0.15),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                    children: const [
                      Icon(Icons.cut, size: 16, color: Colors.purpleAccent),
                      SizedBox(width: 6),
                      Text(
                        'EM ATENDIMENTO',
                        style: TextStyle(color: Colors.purpleAccent, fontSize: 12, fontWeight: FontWeight.bold),
                      ),
                    ],
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                  decoration: BoxDecoration(
                    color: Colors.black.withOpacity(0.08),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.timer_outlined, size: 16, color: Colors.purpleAccent),
                      const SizedBox(width: 4),
                      Text(
                        _formatElapsedTime(inProgressAppt.startedAt),
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13, color: Colors.purpleAccent),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              children: [
                CircleAvatar(
                  radius: 26,
                  backgroundColor: Theme.of(context).colorScheme.primary.withOpacity(0.15),
                  backgroundImage: (inProgressAppt.barberPhoto.isNotEmpty && inProgressAppt.barberPhoto.startsWith('http'))
                      ? NetworkImage(inProgressAppt.barberPhoto)
                      : null,
                  child: (inProgressAppt.barberPhoto.isEmpty || !inProgressAppt.barberPhoto.startsWith('http'))
                      ? Text(
                          inProgressAppt.barberName.isNotEmpty ? inProgressAppt.barberName[0].toUpperCase() : 'B',
                          style: TextStyle(fontWeight: FontWeight.bold, color: Theme.of(context).colorScheme.primary),
                        )
                      : null,
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        inProgressAppt.serviceTypeName,
                        style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 3),
                      Text(
                        'Barbeiro: ${inProgressAppt.barberName}',
                        style: TextStyle(color: Colors.grey[600], fontSize: 14),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            const Divider(),
            const SizedBox(height: 8),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Valor: R\$ ${inProgressAppt.value.toStringAsFixed(2).replaceAll('.', ',')}',
                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                ),
                if (inProgressAppt.barberPhone.isNotEmpty)
                  InkWell(
                    onTap: () => _openWhatsApp(inProgressAppt.barberPhone),
                    borderRadius: BorderRadius.circular(8),
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      child: Row(
                        children: const [
                          Icon(Icons.chat_bubble_outline, size: 16, color: Colors.green),
                          SizedBox(width: 6),
                          Text('WhatsApp', style: TextStyle(color: Colors.green, fontWeight: FontWeight.bold, fontSize: 13)),
                        ],
                      ),
                    ),
                  ),
              ],
            ),
          ],
        ),
      );
    }

    final unratedAppt = _appointments.cast<Appointment?>().firstWhere(
      (a) => a?.status == 2 && (a?.rating == null || a!.rating == 0),
      orElse: () => null,
    );

    if (unratedAppt != null) {
      return Container(
        width: double.infinity,
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: Theme.of(context).cardColor,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: Colors.amber, width: 1.5),
          boxShadow: [
            BoxShadow(
              color: Colors.amber.withOpacity(0.12),
              blurRadius: 14,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: const [
                Icon(Icons.star, color: Colors.amber, size: 20),
                SizedBox(width: 8),
                Text('Atendimento Concluído', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              'Como foi seu corte com ${unratedAppt.barberName}?',
              style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
            ),
            const SizedBox(height: 12),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () => _showRatingModal(unratedAppt),
                icon: const Icon(Icons.star, color: Colors.white, size: 18),
                label: const Text('Avaliar Agora', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.amber[800],
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
              ),
            ),
          ],
        ),
      );
    }

    return _buildNextAppointmentCard();
  }

  Widget _buildNextAppointmentCard() {
    final futureAppointments = _appointments.where((a) => a.status == 0).toList();
    futureAppointments.sort((a, b) => a.date.compareTo(b.date));
    
    final nextAppt = futureAppointments.isNotEmpty ? futureAppointments.first : null;

    if (nextAppt == null) {
      return Container(
        width: double.infinity,
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          color: Theme.of(context).cardColor,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, 4)),
          ],
        ),
        child: Column(
          children: [
            const Icon(Icons.calendar_today, size: 48, color: Colors.grey),
            const SizedBox(height: 16),
            const Text(
              'Você não tem nenhum\nagendamento próximo.',
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey, fontSize: 16),
            ),
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: () async {
                final result = await Navigator.push(
                  context,
                  MaterialPageRoute(builder: (_) => const CustomerBookingPage()),
                );
                if (result == true) {
                  _loadData();
                }
              },
              style: ElevatedButton.styleFrom(
                minimumSize: const Size(double.infinity, 50),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
              child: const Text('Criar Agendamento'),
            ),
          ],
        ),
      );
    }

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Theme.of(context).cardColor,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Theme.of(context).colorScheme.primary.withOpacity(0.2)),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, 4)),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Próximo Agendamento',
            style: TextStyle(color: Colors.grey, fontSize: 14, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Theme.of(context).colorScheme.primary.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(Icons.cut, color: Theme.of(context).colorScheme.primary),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      nextAppt.serviceTypeName,
                      style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'com ${nextAppt.barberName}',
                      style: TextStyle(color: Colors.grey[600], fontSize: 14),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          const Divider(),
          const SizedBox(height: 12),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  const Icon(Icons.calendar_month, size: 16, color: Colors.grey),
                  const SizedBox(width: 4),
                  Text(
                    '${nextAppt.date.day.toString().padLeft(2, '0')}/${nextAppt.date.month.toString().padLeft(2, '0')}',
                    style: const TextStyle(fontWeight: FontWeight.w500),
                  ),
                ],
              ),
              Row(
                children: [
                  const Icon(Icons.schedule, size: 16, color: Colors.grey),
                  const SizedBox(width: 4),
                  Text(
                    nextAppt.hour,
                    style: const TextStyle(fontWeight: FontWeight.w500),
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildIndicatorsGrid() {
    final total = _appointments.length;
    final cancelados = _appointments.where((a) => a.status == 1).length;
    final fazendo = _appointments.where((a) => a.status == 4 || a.status == 0).length;
    final feitos = _appointments.where((a) => a.status == 2).length;

    return GridView.count(
      crossAxisCount: 2,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      mainAxisSpacing: 16,
      crossAxisSpacing: 16,
      childAspectRatio: 1.5,
      children: [
        _buildIndicatorCard('Total', total.toString(), Icons.analytics, Colors.blue),
        _buildIndicatorCard('Concluídos', feitos.toString(), Icons.check_circle, Colors.green),
        _buildIndicatorCard('Agendados', fazendo.toString(), Icons.sync, Colors.orange),
        _buildIndicatorCard('Cancelados', cancelados.toString(), Icons.cancel, Colors.red),
      ],
    );
  }

  Widget _buildIndicatorCard(String title, String value, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Theme.of(context).cardColor,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, 4)),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Icon(icon, color: color, size: 24),
              Text(
                value,
                style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
              ),
            ],
          ),
          const Spacer(),
          Text(
            title,
            style: TextStyle(color: Colors.grey[600], fontSize: 13, fontWeight: FontWeight.w500),
          ),
        ],
      ),
    );
  }
}
