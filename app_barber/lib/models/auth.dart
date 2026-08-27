class LoginRequest {
  final String email;
  final String password;
  final String? tokenFCM;
  final String role;
  final String barbershopId;

  LoginRequest({required this.email, required this.password, this.tokenFCM, required this.role, required this.barbershopId});

  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'password': password,
      if (tokenFCM != null) 'tokenFCM': tokenFCM,
      'role': role,
      'barbershopId': barbershopId
    };
  }
}

class RegisterRequest {
  final String name;
  final String email;
  final String whatsapp;
  final String password;
  final String passwordConfirm;
  final String? barbershopId;

  RegisterRequest({
    required this.name,
    required this.email,
    required this.whatsapp,
    required this.password,
    required this.passwordConfirm,
    this.barbershopId,
  });

  Map<String, dynamic> toJson() {
    return {
      'name': name,
      'email': email,
      'whatsapp': whatsapp.replaceAll(RegExp(r'[^0-9]'), ''),
      'password': password,
      'passwordConfirm': passwordConfirm,
      if (barbershopId != null && barbershopId!.isNotEmpty) 'barbershopId': barbershopId,
    };
  }
}

class UserSession {
  final String token;
  final String refreshToken;
  final String role;
  final String barbershopId;
  final bool passwordResetRequired;
  final String photo;

  UserSession({
    required this.token,
    required this.refreshToken,
    required this.role,
    required this.barbershopId,
    required this.passwordResetRequired,
    required this.photo,
  });
}
