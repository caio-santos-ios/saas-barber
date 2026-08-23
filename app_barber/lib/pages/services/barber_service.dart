import 'package:hive/hive.dart';
import 'package:jwt_decoder/jwt_decoder.dart';

class BarberService {
  final authBox = Hive.box('auth');

  String getUserId() {
    final String token = authBox.get('token', defaultValue: '');
    Map<String, dynamic> decodedToken = JwtDecoder.decode(token);

    return decodedToken["sub"];
  }

  String getUserToken() {
    final String token = authBox.get('token', defaultValue: '');
    return token;
  }
}