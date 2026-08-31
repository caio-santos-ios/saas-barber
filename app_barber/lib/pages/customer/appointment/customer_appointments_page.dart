import 'package:flutter/material.dart';
import 'package:hive/hive.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/appointment.dart';
import 'package:app_barber/repositories/appointment_repository.dart';
import 'package:app_barber/pages/customer/appointment/customer_booking_page.dart';

class CustomerAppointmentsPage extends StatefulWidget {
  const CustomerAppointmentsPage({super.key});

  @override
  State<CustomerAppointmentsPage> createState() => _CustomerAppointmentsPageState();
}

class _CustomerAppointmentsPageState extends State<CustomerAppointmentsPage> {
  late final AppointmentRepository _appointmentRepo;
  List<Appointment> _appointments = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _appointmentRepo = AppointmentRepository(ApiClient());
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);
    try {
      final authBox = Hive.box('auth');
      final barbershopId = (authBox.get('barbershopId', defaultValue: '') as String).trim();
      final list = await _appointmentRepo.getCustomerAppointments(barbershopId);
      if (mounted) setState(() { _appointments = list; _isLoading = false; });
    } catch (e) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).scaffoldBackgroundColor,
      appBar: AppBar(
        title: Text(
          'Meus Agendamentos',
          style: TextStyle(color: Theme.of(context).textTheme.titleLarge?.color),
        ),
        centerTitle: true,
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: IconThemeData(color: Theme.of(context).iconTheme.color),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () async {
          final result = await Navigator.push(
            context,
            MaterialPageRoute(builder: (_) => const CustomerBookingPage()),
          );
          if (result == true) {
            _loadData();
          }
        },
        child: const Icon(Icons.add),
      ),
      body: _isLoading 
        ? const Center(child: CircularProgressIndicator())
        : RefreshIndicator(
            onRefresh: _loadData,
            child: _appointments.isEmpty 
              ? SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  child: Container(
                    height: MediaQuery.of(context).size.height * 0.7,
                    alignment: Alignment.center,
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.calendar_today_outlined,
                          size: 80,
                          color: Theme.of(context).dividerColor,
                        ),
                        const SizedBox(height: 16),
                        const Text(
                          'Nenhum agendamento',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 8),
                        const Text(
                          'Você ainda não possui nenhum\nagendamento marcado.',
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            color: Colors.grey,
                          ),
                        ),
                      ],
                    ),
                  ),
                )
              : ListView.builder(
                  padding: const EdgeInsets.only(left: 20, right: 20, bottom: 100),
                  itemCount: _appointments.length,
                  itemBuilder: (context, index) {
                    final appt = _appointments[index];
                    return _buildAppointmentCard(appt);
                  },
                ),
          ),
    );
  }

  bool _canCancel(Appointment appt) {
    if (appt.status != 1 && appt.status != 0) return false;
    
    try {
      final parts = appt.hour.split(':');
      final apptDateTime = DateTime(
        appt.date.year,
        appt.date.month,
        appt.date.day,
        int.parse(parts[0]),
        int.parse(parts[1]),
      );
      
      final now = DateTime.now();
      final difference = apptDateTime.difference(now);
      
      return difference.inHours >= 24;
    } catch (e) {
      return false;
    }
  }

  Future<void> _cancelAppt(Appointment appt) async {
    final authBox = Hive.box('auth');
    final barbershopId = authBox.get('barbershopId', defaultValue: '');
    setState(() => _isLoading = true);
    final success = await _appointmentRepo.cancelAppointment(appt.id, barbershopId);
    if (!success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Não foi possível cancelar o agendamento. O prazo de 24 horas pode já ter passado ou ocorreu um erro.')),
      );
    }
    _loadData();
  }

  void _showCancelDialog(Appointment appt) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cancelar Agendamento'),
        content: const Text('Tem certeza que deseja cancelar este agendamento?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Não'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              _cancelAppt(appt);
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Sim, cancelar', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
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

  Widget _buildAppointmentCard(Appointment appt) {
    Color statusColor;
    String statusText;
    
    switch (appt.status) {
      case 1:
        statusColor = Colors.red;
        statusText = 'Cancelado';
        break;
      case 2:
        statusColor = Colors.green;
        statusText = 'Concluído';
        break;
      case 3:
        statusColor = Colors.orange;
        statusText = 'Não Realizado';
        break;
      case 4:
        statusColor = Colors.purpleAccent;
        statusText = 'Em Andamento ✂️';
        break;
      default:
        statusColor = Colors.blue;
        statusText = 'Agendado';
    }

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Theme.of(context).cardColor,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: appt.status == 4 ? Colors.purpleAccent : Theme.of(context).dividerColor,
          width: appt.status == 4 ? 2 : 1,
        ),
        boxShadow: [
          BoxShadow(
            color: appt.status == 4 ? Colors.purpleAccent.withOpacity(0.15) : Colors.black.withOpacity(0.05),
            blurRadius: 10,
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
              Text(
                '${appt.date.day.toString().padLeft(2, '0')}/${appt.date.month.toString().padLeft(2, '0')} às ${appt.hour}',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: statusColor.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  statusText,
                  style: TextStyle(color: statusColor, fontSize: 12, fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          const Divider(),
          const SizedBox(height: 12),
          Row(
            children: [
              const Icon(Icons.cut, size: 20, color: Colors.grey),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  appt.serviceTypeName,
                  style: const TextStyle(fontSize: 16),
                ),
              ),
              Text(
                'R\$ ${appt.value.toStringAsFixed(2).replaceAll('.', ',')}',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              const Icon(Icons.person, size: 20, color: Colors.grey),
              const SizedBox(width: 8),
              Text(
                'Barbeiro: ${appt.barberName}',
                style: TextStyle(color: Colors.grey[600]),
              ),
            ],
          ),
          if (appt.status == 2 && (appt.rating == null || appt.rating == 0)) ...[
            const SizedBox(height: 12),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () => _showRatingModal(appt),
                icon: const Icon(Icons.star, color: Colors.white, size: 18),
                label: const Text('Avaliar Atendimento', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.amber[800],
                  padding: const EdgeInsets.symmetric(vertical: 10),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                ),
              ),
            ),
          ] else if (appt.rating != null && appt.rating! > 0) ...[
            const SizedBox(height: 12),
            Row(
              children: [
                ...List.generate(5, (index) => Icon(
                  index < appt.rating! ? Icons.star : Icons.star_border,
                  color: Colors.amber,
                  size: 18,
                )),
                const SizedBox(width: 8),
                Text(
                  'Avaliado (${appt.rating} estrelas)',
                  style: const TextStyle(color: Colors.grey, fontSize: 12),
                ),
              ],
            ),
          ] else if (_canCancel(appt)) ...[
            const SizedBox(height: 12),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton(
                onPressed: () => _showCancelDialog(appt),
                style: OutlinedButton.styleFrom(
                  foregroundColor: Colors.red,
                  side: const BorderSide(color: Colors.red),
                ),
                child: const Text('Cancelar Agendamento'),
              ),
            ),
          ] else if (appt.status == 0) ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.orange.withOpacity(0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: const Row(
                children: [
                  Icon(Icons.info_outline, color: Colors.orange, size: 20),
                  SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      'Faltam menos de 24h, o cancelamento pelo app não está mais disponível.',
                      style: TextStyle(color: Colors.orange, fontSize: 13),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}
