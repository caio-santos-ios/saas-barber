import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/auth.dart';
import 'package:dio/dio.dart';
import 'package:hive_flutter/hive_flutter.dart';

class AuthRepository {
  final ApiClient apiClient;

  AuthRepository(this.apiClient);

  Future<UserSession?> login(LoginRequest request) async {
    final response = await apiClient.dio.post(
      '/auth/login',
      data: request.toJson(),
    );

    if (response.statusCode == 200) {
      final data = response.data['data'] ?? response.data;
      final token = data['token'] ?? '';
      final refreshToken = data['refreshToken'] ?? '';
      final photo = data['photo'] ?? '';
      final role = data['role'] ?? '';

      final session = UserSession(
        token: token,
        refreshToken: refreshToken,
        photo: photo,
        barbershopId: "",
        role: role,
        passwordResetRequired: false
      );

      final authBox = Hive.box('auth');
      await authBox.put('token', token);
      await authBox.put('refreshToken', refreshToken);
      await authBox.put('photo', photo);
      await authBox.put('role', role);

      return session;
    }
    return null;
  }

  Future<bool> registerCustomer(RegisterRequest request) async {
    try {
      final response = await apiClient.dio.post(
        '/auth/customers/register',
        data: request.toJson(),
      );
      return response.statusCode == 200 || response.statusCode == 201;
    } on DioException catch (e) {
      throw Exception(e.response?.data['message'] ?? 'Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.');
    }
  }

  Future<bool> forgotPassword(String email) async {
    try {
      final response = await apiClient.dio.post(
        '/auth/reset-password',
        data: {'email': email, 'originUrl': 'https://app.barber.com'}, // TODO: Set dynamically if needed
      );
      return response.statusCode == 200;
    } on DioException catch (e) {
      throw Exception(e.response?.data['message'] ?? 'Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.');
    }
  }

  Future<bool> updatePassword(String newPassword) async {
    try {
      final response = await apiClient.dio.post(
        '/auth/update-password',
        data: {'password': newPassword},
      );
      if (response.statusCode == 200) {
        final authBox = Hive.box('auth');
        await authBox.put('passwordResetRequired', false);
        return true;
      }
      return false;
    } on DioException catch (e) {
      throw Exception(e.response?.data['message'] ?? 'Erro.');
    }
  }

  Future<bool> confirmResetPassword(String code, String newPassword) async {
    try {
      final response = await apiClient.dio.post(
        '/auth/confirm-reset-password',
        data: {'code': code, 'newPassword': newPassword},
      );
      return response.statusCode == 200;
    } on DioException catch (e) {
      throw Exception(e.response?.data['message'] ?? 'Ocorreu um erro inesperado.');
    }
  
    try {
      final response = await apiClient.dio.post(
        '/auth/update-password',
        data: {'password': newPassword},
      );
      
      if (response.statusCode == 200) {
        final authBox = Hive.box('auth');
        await authBox.put('passwordResetRequired', false);
        return true;
      }
      return false;
    } on DioException catch (e) {
      throw Exception(e.response?.data['message'] ?? 'Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.');
    }
  }

  void logout() {
    final authBox = Hive.box('auth');
    authBox.delete('token');
    authBox.delete('refreshToken');
    authBox.delete('role');
    authBox.delete('barbershopId');
    authBox.delete('passwordResetRequired');
    authBox.delete('photo');
  }
}


