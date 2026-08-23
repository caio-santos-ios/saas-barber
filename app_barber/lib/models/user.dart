class User {
  final String id;
  final String name;
  final String email;
  final String whatsapp;
  final String photo;
  final String role;

  User({
    required this.id,
    required this.name,
    required this.email,
    required this.whatsapp,
    required this.photo,
    required this.role,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] ?? '',
      name: json['name'] ?? '',
      email: json['email'] ?? '',
      whatsapp: json['whatsapp'] ?? '',
      photo: json['photo'] ?? '',
      role: json['role']?.toString() ?? '',
    );
  }
}
