import 'package:hive/hive.dart';
import 'package:jwt_decoder/jwt_decoder.dart';

class BarberService {
  final authBox = Hive.box('auth');

  String getUserId() {
    final String token = authBox.get('token', defaultValue: '');
    if (token.isEmpty) return authBox.get('userId', defaultValue: '');
    try {
      Map<String, dynamic> decodedToken = JwtDecoder.decode(token);
      return decodedToken["sub"] ?? decodedToken["userId"] ?? "";
    } catch (_) {
      return authBox.get('userId', defaultValue: '');
    }
  }

  String getBarbershopId() {
    final String barbershopId = authBox.get('barbershopId', defaultValue: '');
    if (barbershopId.isNotEmpty) return barbershopId;
    final String token = authBox.get('token', defaultValue: '');
    if (token.isNotEmpty) {
      try {
        Map<String, dynamic> decodedToken = JwtDecoder.decode(token);
        return decodedToken["barbershopId"] ?? "";
      } catch (_) {}
    }
    return '';
  }

  String getUserToken() {
    final String token = authBox.get('token', defaultValue: '');
    return token;
  }
}