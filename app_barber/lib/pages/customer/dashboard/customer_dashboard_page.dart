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
  
  String _customerName = 'Cliente';
  String _customerPhoto = '';

  @override
  void initState() {
    super.initState();
    final apiClient = ApiClient();
    _barbershopRepo = BarbershopRepository(apiClient);
    _appointmentRepo = AppointmentRepository(apiClient);
    _loadData();
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
                const SizedBox(height: 24),
                _buildNextAppointmentCard(),
                const SizedBox(height: 16),
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
    if (_customerPhoto.isEmpty) return null;
    if (_customerPhoto.startsWith('http')) return NetworkImage(_customerPhoto);
    if (_customerPhoto.startsWith('data:image')) {
      try {
        final base64Str = _customerPhoto.split(',').last;
        return MemoryImage(base64Decode(base64Str));
      } catch (e) {
        return null;
      }
    }
    return null;
  }

  Widget _buildHeader() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        GestureDetector(
          onTap: () {
            // Ir para tela de perfil
          },
          child: CircleAvatar(
            radius: 24,
            backgroundColor: Theme.of(context).dividerColor,
            backgroundImage: _getProfileImage(),
            child: _customerPhoto.isEmpty ? Icon(Icons.person, color: Theme.of(context).iconTheme.color) : null,
          ),
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

  Widget _buildNextAppointmentCard() {
    // Filtra apenas agendamentos futuros (status 1 = marcado)
    final futureAppointments = _appointments.where((a) => a.status == 1).toList();
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
    final cancelados = _appointments.where((a) => a.status == 2).length;
    final fazendo = _appointments.where((a) => a.status == 1).length; // 1 = marcado/fazendo
    final feitos = _appointments.where((a) => a.status == 3).length; // 3 = finalizado

    return GridView.count(
      crossAxisCount: 2,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      mainAxisSpacing: 16,
      crossAxisSpacing: 16,
      childAspectRatio: 1.5,
      children: [
        _buildIndicatorCard('Total', total.toString(), Icons.analytics, Colors.blue),
        _buildIndicatorCard('Feitos', feitos.toString(), Icons.check_circle, Colors.green),
        _buildIndicatorCard('Em andamento', fazendo.toString(), Icons.sync, Colors.orange),
        _buildIndicatorCard('Cancelados', cancelados.toString(), Icons.cancel, Colors.red),
      ],
    );
  }

  Widget _buildIndicatorCard(String title, String value, IconData icon, Color color) {
    return GestureDetector(
      onTap: () {
        // Redirecionar para tela de Meus Agendamentos com filtro
      },
      child: Container(
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
      ),
    );
  }
}
