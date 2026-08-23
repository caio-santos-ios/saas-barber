import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/barbershop.dart';
import 'package:app_barber/models/service_type.dart';
import 'package:app_barber/models/user.dart';

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

  Future<List<User>> getBarbershopTeam() async {
    try {
      final response = await apiClient.dio.get('/users/barbers');
      if (response.statusCode == 200 && response.data['data'] != null) {
        final List data = response.data['data'];
        return data.map((json) => User.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  Future<List<ServiceType>> getBarbershopServices() async {
    try {
      final response = await apiClient.dio.get('/services_types/barbers');
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
