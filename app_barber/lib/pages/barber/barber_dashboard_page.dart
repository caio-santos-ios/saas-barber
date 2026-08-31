import 'package:flutter/material.dart';
import 'package:hive/hive.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/appointment.dart';
import 'package:app_barber/repositories/appointment_repository.dart';
import 'package:intl/intl.dart';
import 'package:app_barber/pages/barber/notification/barber_notifications_page.dart';

class BarberDashboardPage extends StatefulWidget {
  const BarberDashboardPage({super.key});

  @override
  State<BarberDashboardPage> createState() => _BarberDashboardPageState();
}

class _BarberDashboardPageState extends State<BarberDashboardPage> {
  late final AppointmentRepository _appointmentRepo;
  List<Appointment> _appointments = [];
  bool _isLoading = true;
  DateTime _selectedDate = DateTime.now();

  @override
  void initState() {
    super.initState();
    _appointmentRepo = AppointmentRepository(ApiClient());
    _loadData();
  }

  Future<void> _loadData() async {
    try {
      final list = await _appointmentRepo.getBarberAppointments();
      
      if (mounted) {
        setState(() {
          _appointments = list;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  List<Appointment> get _filteredAppointments {
    final filtered = _appointments.where((a) {
      return a.date.year == _selectedDate.year &&
             a.date.month == _selectedDate.month &&
             a.date.day == _selectedDate.day;
    }).toList();
    
    filtered.sort((a, b) => a.hour.compareTo(b.hour));
    return filtered;
  }

  Future<void> _updateStatus(Appointment appt, int newStatus) async {
    final authBox = Hive.box('auth');
    final barbershopId = authBox.get('barbershopId', defaultValue: '');
    
    setState(() => _isLoading = true);
    final success = await _appointmentRepo.updateAppointmentStatus(appt.id, newStatus, barbershopId);
    if (!success && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Erro ao atualizar o agendamento.')),
      );
    }
    _loadData();
  }

  Future<void> _startAppointment(Appointment appt) async {
    setState(() => _isLoading = true);
    final success = await _appointmentRepo.startAppointment(appt.id);
    if (mounted) {
      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Atendimento iniciado com sucesso! ✂️'), backgroundColor: Colors.green),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Erro ao iniciar atendimento.'), backgroundColor: Colors.red),
        );
      }
    }
    _loadData();
  }

  Future<void> _finishAppointment(Appointment appt) async {
    setState(() => _isLoading = true);
    final success = await _appointmentRepo.finishAppointment(appt.id);
    if (mounted) {
      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Atendimento finalizado com sucesso! ⭐'), backgroundColor: Colors.green),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Erro ao finalizar atendimento.'), backgroundColor: Colors.red),
        );
      }
    }
    _loadData();
  }

  void _showConcludeDialog(Appointment appt) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Finalizar Atendimento'),
        content: const Text('Deseja marcar este serviço como concluído? O cliente receberá a notificação para avaliação.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              _finishAppointment(appt);
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.green),
            child: const Text('Sim, finalizar', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
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
              _updateStatus(appt, 1);
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Sim, cancelar', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).scaffoldBackgroundColor,
      appBar: AppBar(
        title: Text(
          'Minha Agenda',
          style: TextStyle(color: Theme.of(context).textTheme.titleLarge?.color),
        ),
        centerTitle: true,
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: IconThemeData(color: Theme.of(context).iconTheme.color),
        actions: [
          IconButton(
            icon: const Icon(Icons.notifications_none, size: 28),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const BarberNotificationsPage()),
              );
            },
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: Column(
        children: [
          _buildDateSelector(),
          Expanded(
            child: _isLoading 
              ? const Center(child: CircularProgressIndicator())
              : RefreshIndicator(
                  onRefresh: _loadData,
                  child: _filteredAppointments.isEmpty 
                    ? _buildEmptyState()
                    : ListView.builder(
                        padding: const EdgeInsets.all(20),
                        itemCount: _filteredAppointments.length,
                        itemBuilder: (context, index) {
                          final appt = _filteredAppointments[index];
                          return _buildAppointmentCard(appt);
                        },
                      ),
                ),
          ),
        ],
      ),
    );
  }

  Widget _buildDateSelector() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          IconButton(
            icon: const Icon(Icons.chevron_left),
            onPressed: () {
              setState(() {
                _selectedDate = _selectedDate.subtract(const Duration(days: 1));
              });
            },
          ),
          Column(
            children: [
              Text(
                DateFormat('dd/MM/yyyy').format(_selectedDate),
                style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 4),
              Text(
                _isToday(_selectedDate) ? 'Hoje' : 
                _isTomorrow(_selectedDate) ? 'Amanhã' :
                _isYesterday(_selectedDate) ? 'Ontem' : '',
                style: TextStyle(color: Theme.of(context).colorScheme.primary, fontWeight: FontWeight.w600),
              ),
            ],
          ),
          IconButton(
            icon: const Icon(Icons.chevron_right),
            onPressed: () {
              setState(() {
                _selectedDate = _selectedDate.add(const Duration(days: 1));
              });
            },
          ),
        ],
      ),
    );
  }

  bool _isToday(DateTime date) {
    final now = DateTime.now();
    return date.year == now.year && date.month == now.month && date.day == now.day;
  }
  
  bool _isTomorrow(DateTime date) {
    final tomorrow = DateTime.now().add(const Duration(days: 1));
    return date.year == tomorrow.year && date.month == tomorrow.month && date.day == tomorrow.day;
  }
  
  bool _isYesterday(DateTime date) {
    final yesterday = DateTime.now().subtract(const Duration(days: 1));
    return date.year == yesterday.year && date.month == yesterday.month && date.day == yesterday.day;
  }

  Widget _buildEmptyState() {
    return SingleChildScrollView(
      physics: const AlwaysScrollableScrollPhysics(),
      child: Container(
        height: MediaQuery.of(context).size.height * 0.6,
        alignment: Alignment.center,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.event_busy_outlined,
              size: 80,
              color: Theme.of(context).dividerColor,
            ),
            const SizedBox(height: 16),
            const Text(
              'Agenda Livre',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            const Text(
              'Você não tem agendamentos\npara esta data.',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Colors.grey,
              ),
            ),
          ],
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

    final isInProgress = appt.status == 4;

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Theme.of(context).cardColor,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: isInProgress ? Colors.purpleAccent : Theme.of(context).dividerColor,
          width: isInProgress ? 2 : 1,
        ),
        boxShadow: [
          BoxShadow(
            color: isInProgress ? Colors.purpleAccent.withOpacity(0.15) : Colors.black.withOpacity(0.05),
            blurRadius: isInProgress ? 12 : 10,
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
                'Horário: ${appt.hour}',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
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
              const Icon(Icons.person_outline, size: 20, color: Colors.grey),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  appt.customerName,
                  style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w500),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              const Icon(Icons.cut, size: 20, color: Colors.grey),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  appt.serviceTypeName,
                  style: TextStyle(color: Colors.grey[600]),
                ),
              ),
              Text(
                'R\$ ${appt.value.toStringAsFixed(2).replaceAll('.', ',')}',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
            ],
          ),
          if (appt.status == 4) ...[
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: () => _showCancelDialog(appt),
                    style: OutlinedButton.styleFrom(
                      foregroundColor: Colors.red,
                      side: const BorderSide(color: Colors.red),
                      padding: const EdgeInsets.symmetric(vertical: 12),
                    ),
                    child: const Text('Cancelar'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton.icon(
                    onPressed: () => _showConcludeDialog(appt),
                    icon: const Icon(Icons.check, color: Colors.white),
                    label: const Text('Finalizar', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green,
                      padding: const EdgeInsets.symmetric(vertical: 12),
                    ),
                  ),
                ),
              ],
            ),
          ] else if (appt.status == 0) ...[
            const SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: () => _showCancelDialog(appt),
                    style: OutlinedButton.styleFrom(
                      foregroundColor: Colors.red,
                      side: const BorderSide(color: Colors.red),
                      padding: const EdgeInsets.symmetric(vertical: 12),
                    ),
                    child: const Text('Cancelar'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton.icon(
                    onPressed: () => _startAppointment(appt),
                    icon: const Icon(Icons.play_arrow, color: Colors.white),
                    label: const Text('Iniciar', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Theme.of(context).colorScheme.primary,
                      padding: const EdgeInsets.symmetric(vertical: 12),
                    ),
                  ),
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }
}
