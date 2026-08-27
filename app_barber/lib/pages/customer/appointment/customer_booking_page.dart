import 'package:flutter/material.dart';
import 'package:hive/hive.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/user.dart';
import 'package:app_barber/models/service_type.dart';
import 'package:app_barber/models/appointment.dart';
import 'package:app_barber/repositories/barbershop_repository.dart';
import 'package:app_barber/repositories/appointment_repository.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:intl/intl.dart';

class CustomerBookingPage extends StatefulWidget {
  const CustomerBookingPage({super.key});

  @override
  State<CustomerBookingPage> createState() => _CustomerBookingPageState();
}

class _CustomerBookingPageState extends State<CustomerBookingPage> {
  int _currentStep = 0;
  bool _isLoading = false;
  bool _isLoadingSlots = false;

  late final BarbershopRepository _barbershopRepo;
  late final AppointmentRepository _appointmentRepo;

  List<User> _barbers = [];
  List<ServiceType> _services = [];
  List<String> _availableSlots = [];

  User? _selectedBarber;
  ServiceType? _selectedService;
  DateTime? _selectedDate;
  String? _selectedHour;
  final TextEditingController _notesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    final apiClient = ApiClient();
    _barbershopRepo = BarbershopRepository(apiClient);
    _appointmentRepo = AppointmentRepository(apiClient);

    _loadInitialData();
  }

  Future<void> _loadInitialData() async {
    setState(() => _isLoading = true);

    final authBox = Hive.box('auth');
    final barbershopId = (authBox.get('barbershopId', defaultValue: '') as String).trim();

    if (barbershopId.isEmpty) {
      if (mounted) {
        setState(() {
          _barbers = [];
          _services = [];
          _isLoading = false;
        });
      }
      return;
    }

    final futures = await Future.wait([
      _barbershopRepo.getBarbershopTeam(barbershopId),
      _barbershopRepo.getBarbershopServices(barbershopId),
    ]);

    if (mounted) {
      setState(() {
        _barbers = futures[0] as List<User>;
        _services = futures[1] as List<ServiceType>;
        _isLoading = false;
      });
    }
  }

  Future<void> _loadSlots() async {
    if (_selectedBarber == null || _selectedDate == null) return;
    
    setState(() => _isLoadingSlots = true);
    final authBox = Hive.box('auth');
    final barbershopId = authBox.get('barbershopId', defaultValue: '');

    final slots = await _appointmentRepo.getAvailableSlots(
      _selectedBarber!.id, 
      _selectedDate!, 
      barbershopId
    );

    if (mounted) {
      setState(() {
        _availableSlots = slots;
        
        if (_selectedDate!.day == DateTime.now().day &&
            _selectedDate!.month == DateTime.now().month &&
            _selectedDate!.year == DateTime.now().year) {
          final now = DateTime.now();
          _availableSlots.removeWhere((hour) {
            final parts = hour.split(':');
            final slotHour = int.parse(parts[0]);
            final slotMinute = int.parse(parts[1]);
            if (slotHour < now.hour) return true;
            if (slotHour == now.hour && slotMinute <= now.minute) return true;
            return false;
          });
        }

        _selectedHour = null;
        _isLoadingSlots = false;
      });
    }
  }

  Future<void> _submitAppointment() async {
    if (_selectedBarber == null || _selectedService == null || _selectedDate == null || _selectedHour == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Por favor, preencha todos os passos antes de confirmar.'), backgroundColor: Colors.orange),
      );
      return;
    }

    setState(() => _isLoading = true);
    final authBox = Hive.box('auth');
    final barbershopId = authBox.get('barbershopId', defaultValue: '');
    final token = authBox.get('token', defaultValue: '');
    
    String customerId = '';
    String customerName = 'Cliente';
    
    if (token.isNotEmpty) {
      Map<String, dynamic> decodedToken = JwtDecoder.decode(token);
      customerId = decodedToken['sub'] ?? 
                   decodedToken['userId'] ?? 
                   decodedToken['nameid'] ?? 
                   decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? 
                   '';
      customerName = decodedToken['name'] ?? 
                     decodedToken['unique_name'] ?? 
                     decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? 
                     'Cliente';
    }

    final request = CreateAppointmentRequest(
      date: _selectedDate!,
      hour: _selectedHour!,
      notes: _notesController.text,
      barberId: _selectedBarber!.id,
      barberName: _selectedBarber!.name,
      customerId: customerId,
      serviceId: _selectedService!.id,
      serviceTypeId: _selectedService!.id,
      customerName: customerName,
      serviceTypeName: _selectedService!.name,
      value: _selectedService!.value,
    );

    final success = await _appointmentRepo.createAppointment(request, barbershopId);

    if (mounted) {
      setState(() => _isLoading = false);
      if (success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Agendamento realizado com sucesso!'), backgroundColor: Colors.green),
        );
        Navigator.of(context).pop(true);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Erro ao criar agendamento.'), backgroundColor: Colors.red),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Novo Agendamento', style: TextStyle(color: Theme.of(context).textTheme.titleLarge?.color)),
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: IconThemeData(color: Theme.of(context).iconTheme.color),
      ),
      body: _isLoading && _currentStep == 0
        ? const Center(child: CircularProgressIndicator())
        : Stepper(
            type: StepperType.vertical,
            currentStep: _currentStep,
            onStepTapped: (step) => setState(() => _currentStep = step),
            onStepContinue: () {
              if (_currentStep < 4) {
                if (_currentStep == 0 && _selectedBarber == null) return;
                if (_currentStep == 1 && _selectedService == null) return;
                if (_currentStep == 2 && _selectedDate == null) return;
                if (_currentStep == 3 && _selectedHour == null) return;
                setState(() => _currentStep += 1);
              } else {
                _submitAppointment();
              }
            },
            onStepCancel: () {
              if (_currentStep > 0) {
                setState(() => _currentStep -= 1);
              } else {
                Navigator.pop(context);
              }
            },
            controlsBuilder: (context, details) {
              final isLastStep = _currentStep == 4;
              if (!isLastStep) return const SizedBox.shrink(); // Hide controls on other steps
              
              return Padding(
                padding: const EdgeInsets.only(top: 24.0),
                child: Row(
                  children: [
                    Expanded(
                      child: ElevatedButton(
                        onPressed: _isLoading ? null : details.onStepContinue,
                        style: ElevatedButton.styleFrom(
                          padding: const EdgeInsets.symmetric(vertical: 16),
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                        ),
                        child: _isLoading
                            ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                            : const Text('Confirmar Agendamento'),
                      ),
                    ),
                    const SizedBox(width: 16),
                    TextButton(
                      onPressed: details.onStepCancel,
                      child: const Text('Voltar', style: TextStyle(color: Colors.grey)),
                    ),
                  ],
                ),
              );
            },
            steps: [
              _buildStepBarber(),
              _buildStepService(),
              _buildStepDate(),
              _buildStepHour(),
              _buildStepConfirm(),
            ],
          ),
    );
  }

  Step _buildStepBarber() {
    return Step(
      title: const Text('Selecione o Barbeiro', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
      isActive: _currentStep >= 0,
      state: _selectedBarber != null ? StepState.complete : StepState.indexed,
      content: _barbers.isEmpty 
        ? const Padding(
            padding: EdgeInsets.symmetric(vertical: 20),
            child: Text('Nenhum barbeiro encontrado nesta barbearia.', style: TextStyle(color: Colors.red)),
          )
        : SizedBox(
            height: 120,
            child: ListView.builder(
          scrollDirection: Axis.horizontal,
          itemCount: _barbers.length,
          itemBuilder: (context, index) {
            final barber = _barbers[index];
            final isSelected = _selectedBarber?.id == barber.id;
            return GestureDetector(
              onTap: () {
                setState(() {
                  _selectedBarber = barber;
                  // reset subsequent steps
                  _selectedDate = null;
                  _selectedHour = null;
                });
                Future.delayed(const Duration(milliseconds: 300), () {
                  if (mounted) setState(() => _currentStep = 1);
                });
              },
              child: Container(
                width: 90,
                margin: const EdgeInsets.only(right: 12),
                decoration: BoxDecoration(
                  color: isSelected ? Theme.of(context).colorScheme.primary.withOpacity(0.1) : Theme.of(context).cardColor,
                  border: Border.all(color: isSelected ? Theme.of(context).colorScheme.primary : Colors.grey[300]!, width: 2),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    CircleAvatar(
                      radius: 30,
                      backgroundColor: Colors.grey[200],
                      backgroundImage: barber.photo.isNotEmpty ? NetworkImage(barber.photo) : null,
                      child: barber.photo.isEmpty ? const Icon(Icons.person, color: Colors.grey) : null,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      barber.name.split(' ').first,
                      style: TextStyle(
                        fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                        color: isSelected ? Theme.of(context).colorScheme.primary : Theme.of(context).textTheme.bodyMedium?.color,
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }

  Step _buildStepService() {
    return Step(
      title: const Text('Selecione o Serviço', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
      isActive: _currentStep >= 1,
      state: _selectedService != null ? StepState.complete : StepState.indexed,
      content: _services.isEmpty
        ? const Padding(
            padding: EdgeInsets.symmetric(vertical: 20),
            child: Text('Nenhum serviço encontrado.', style: TextStyle(color: Colors.red)),
          )
        : ListView.builder(
        shrinkWrap: true,
        physics: const NeverScrollableScrollPhysics(),
        itemCount: _services.length,
        itemBuilder: (context, index) {
          final service = _services[index];
          final isSelected = _selectedService?.id == service.id;
          return GestureDetector(
            onTap: () {
              setState(() => _selectedService = service);
              Future.delayed(const Duration(milliseconds: 300), () {
                if (mounted) setState(() => _currentStep = 2);
              });
            },
            child: Container(
              margin: const EdgeInsets.only(bottom: 12),
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: isSelected ? Theme.of(context).colorScheme.primary.withOpacity(0.05) : Theme.of(context).cardColor,
                border: Border.all(color: isSelected ? Theme.of(context).colorScheme.primary : Colors.grey[300]!),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(service.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                        const SizedBox(height: 4),
                        Row(
                          children: [
                            const Icon(Icons.schedule, size: 14, color: Colors.grey),
                            const SizedBox(width: 4),
                            Text('${service.durationMinutes} min', style: const TextStyle(color: Colors.grey)),
                          ],
                        ),
                      ],
                    ),
                  ),
                  Text(
                    'R\$ ${service.value.toStringAsFixed(2).replaceAll('.', ',')}',
                    style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Theme.of(context).colorScheme.primary),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  Step _buildStepDate() {
    return Step(
      title: const Text('Escolha a Data', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
      isActive: _currentStep >= 2,
      state: _selectedDate != null ? StepState.complete : StepState.indexed,
      content: SizedBox(
        height: 80,
        child: ListView.builder(
          scrollDirection: Axis.horizontal,
          itemCount: 15, // Next 15 days
          itemBuilder: (context, index) {
            final date = DateTime.now().add(Duration(days: index));
            final isSelected = _selectedDate != null && 
                               _selectedDate!.day == date.day && 
                               _selectedDate!.month == date.month;
            
            final dayName = DateFormat('E', 'pt_BR').format(date).replaceAll('.', ''); // seg, ter, etc
            
            return GestureDetector(
              onTap: () {
                setState(() {
                  _selectedDate = date;
                  _selectedHour = null; // reset hour
                });
                _loadSlots();
                Future.delayed(const Duration(milliseconds: 300), () {
                  if (mounted) setState(() => _currentStep = 3);
                });
              },
              child: Container(
                width: 60,
                margin: const EdgeInsets.only(right: 12),
                decoration: BoxDecoration(
                  color: isSelected ? Theme.of(context).colorScheme.primary : Theme.of(context).cardColor,
                  border: Border.all(color: isSelected ? Theme.of(context).colorScheme.primary : Colors.grey[300]!),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      dayName.toUpperCase(),
                      style: TextStyle(
                        fontSize: 12,
                        color: isSelected ? Colors.white70 : Colors.grey[600],
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      date.day.toString(),
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                        color: isSelected ? Colors.white : Theme.of(context).textTheme.bodyMedium?.color,
                      ),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }

  Step _buildStepHour() {
    return Step(
      title: const Text('Escolha o Horário', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
      isActive: _currentStep >= 3,
      state: _selectedHour != null ? StepState.complete : StepState.indexed,
      content: SizedBox(
        width: double.infinity,
        child: _isLoadingSlots 
          ? const Padding(padding: EdgeInsets.all(20), child: Center(child: CircularProgressIndicator()))
          : _availableSlots.isEmpty
            ? const Padding(
                padding: EdgeInsets.all(20),
                child: Center(child: Text('Nenhum horário disponível para este dia.', style: TextStyle(color: Colors.red))),
              )
            : Wrap(
                spacing: 12,
                runSpacing: 12,
                children: _availableSlots.map((hour) {
                  final isSelected = _selectedHour == hour;
                  return GestureDetector(
                    onTap: () {
                      setState(() => _selectedHour = hour);
                      Future.delayed(const Duration(milliseconds: 300), () {
                        if (mounted) setState(() => _currentStep = 4);
                      });
                    },
                    child: Container(
                      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                      decoration: BoxDecoration(
                        color: isSelected ? Theme.of(context).colorScheme.primary : Theme.of(context).cardColor,
                        border: Border.all(color: isSelected ? Theme.of(context).colorScheme.primary : Colors.grey[300]!),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Text(
                        hour,
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: isSelected ? Colors.white : Theme.of(context).textTheme.bodyMedium?.color,
                        ),
                      ),
                    ),
                  );
                }).toList(),
              ),
      ),
    );
  }

  Step _buildStepConfirm() {
    return Step(
      title: const Text('Resumo e Confirmação', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
      isActive: _currentStep >= 4,
      state: StepState.indexed,
      content: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Theme.of(context).cardColor,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: Theme.of(context).dividerColor),
            ),
            child: Column(
              children: [
                _buildSummaryRow('Barbeiro', _selectedBarber?.name ?? ''),
                const Divider(height: 24),
                _buildSummaryRow('Serviço', _selectedService?.name ?? ''),
                const Divider(height: 24),
                _buildSummaryRow('Data', _selectedDate != null ? DateFormat('dd/MM/yyyy').format(_selectedDate!) : ''),
                const Divider(height: 24),
                _buildSummaryRow('Horário', _selectedHour ?? ''),
                const Divider(height: 24),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('Total', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
                    Text(
                      'R\$ ${(_selectedService?.value ?? 0).toStringAsFixed(2).replaceAll('.', ',')}',
                      style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Theme.of(context).colorScheme.primary),
                    ),
                  ],
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),
          const Text('Observações (opcional)', style: TextStyle(fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          TextField(
            controller: _notesController,
            maxLines: 3,
            decoration: InputDecoration(
              hintText: 'Alguma preferência ou detalhe?',
              filled: true,
              fillColor: Theme.of(context).cardColor,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide(color: Theme.of(context).dividerColor),
              ),
              enabledBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
                borderSide: BorderSide(color: Theme.of(context).dividerColor),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSummaryRow(String label, String value) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: const TextStyle(color: Colors.grey)),
        Text(value, style: const TextStyle(fontWeight: FontWeight.bold)),
      ],
    );
  }
}
