import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/notification.dart';
import 'package:dio/dio.dart';

class NotificationRepository {
  final ApiClient apiClient;

  NotificationRepository(this.apiClient);

  Future<List<AppNotification>> getNotifications() async {
    try {
      final response = await apiClient.dio.get('/notifications');
      if (response.statusCode == 200) {
        final data = response.data['data'] as List;
        return data.map((json) => AppNotification.fromJson(json)).toList();
      }
      return [];
    } on DioException catch (e) {
      return [];
    }
  }
}
