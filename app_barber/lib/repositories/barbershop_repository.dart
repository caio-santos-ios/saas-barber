import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/barbershop.dart';
import 'package:app_barber/models/service_type.dart';
import 'package:app_barber/models/user.dart';
import 'package:hive/hive.dart';

class BarbershopRepository {
  final ApiClient apiClient;

  BarbershopRepository(this.apiClient);

  Future<Barbershop?> getBarbershop(String id) async {
    try {
      final response = await apiClient.dio.get('/barbershops/$id?barbershopId=$id');
      if (response.statusCode == 200 && response.data['data'] != null) {
        return Barbershop.fromJson(response.data['data']);
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  Future<Barbershop?> getBarbershopByCode(String code) async {
    try {
      final cleanCode = code.trim();
      final response = await apiClient.dio.get('/barbershops/by-code/$cleanCode');
      if (response.statusCode == 200 && response.data['data'] != null) {
        return Barbershop.fromJson(response.data['data']);
      }
      return null;
    } catch (e) {
      return null;
    }
  }

  Future<List<User>> getBarbershopTeam([String? barbershopId]) async {
    try {
      final authBox = Hive.box('auth');
      final bId = (barbershopId ?? (authBox.get('barbershopId', defaultValue: '') as String)).trim();
      if (bId.isEmpty) return [];

      final response = await apiClient.dio.get('/users/barbers?barbershopId=$bId');
      if (response.statusCode == 200 && response.data['data'] != null) {
        final List data = response.data['data'];
        return data.map((json) => User.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<List<ServiceType>> getBarbershopServices([String? barbershopId]) async {
    try {
      final authBox = Hive.box('auth');
      final bId = (barbershopId ?? (authBox.get('barbershopId', defaultValue: '') as String)).trim();
      if (bId.isEmpty) return [];

      final response = await apiClient.dio.get('/services_types?barbershopId=$bId&deleted=false');
      if (response.statusCode == 200 && response.data['data'] != null) {
        final List data = response.data['data'];
        return data.map((json) => ServiceType.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }
}
