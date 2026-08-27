import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/appointment.dart';
import 'package:hive/hive.dart';

class AppointmentRepository {
  final ApiClient apiClient;

  AppointmentRepository(this.apiClient);

  Future<List<Appointment>> getCustomerAppointments([String? barbershopId]) async {
    try {
      final authBox = Hive.box('auth');
      final bId = barbershopId ?? (authBox.get('barbershopId', defaultValue: '') as String);
      final response = await apiClient.dio.get('/appointments?barbershopId=$bId');

      if (response.statusCode == 200 && response.data['data'] != null) {
        final List data = response.data['data'];
        return data.map((json) => Appointment.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<List<Appointment>> getBarberAppointments([String? barbershopId]) async {
    try {
      final authBox = Hive.box('auth');
      final bId = barbershopId ?? (authBox.get('barbershopId', defaultValue: '') as String);
      final response = await apiClient.dio.get('/appointments?barbershopId=$bId');
      if (response.statusCode == 200 && response.data['data'] != null) {
        final List data = response.data['data'];
        return data.map((json) => Appointment.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<List<String>> getAvailableSlots(String barberId, DateTime date, String barbershopId) async {
    try {
      final dateStr = date.toIso8601String().split('T')[0];
      final response = await apiClient.dio.get('/appointments/availability?barberId=$barberId&date=$dateStr&barbershopId=$barbershopId');
      if (response.statusCode == 200 && response.data['data'] != null) {
        final List data = response.data['data'];
        return data.cast<String>();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<bool> createAppointment(CreateAppointmentRequest request, String barbershopId) async {
    try {
      final response = await apiClient.dio.post(
        '/appointments?barbershopId=$barbershopId',
        data: request.toJson(),
      );
      return response.statusCode == 201 || response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> cancelAppointment(String appointmentId, String barbershopId) async {
    try {
      final response = await apiClient.dio.put(
        '/appointments/$appointmentId/status?barbershopId=$barbershopId',
        data: {
          'id': appointmentId,
          'status': 2,
          'cancelNotes': 'Cancelado pelo cliente',
        },
      );
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<bool> updateAppointmentStatus(String appointmentId, int status, String barbershopId) async {
    try {
      final response = await apiClient.dio.put(
        '/appointments/status',
        data: {
          'id': appointmentId,
          'status': status,
        },
      );
      print(response.data);
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}
