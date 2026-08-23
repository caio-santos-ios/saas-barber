class Barbershop {
  final String id;
  final String name;
  final String document;
  final String email;
  final String phone;
  final String whatsapp;
  final String logo;

  Barbershop({
    required this.id,
    required this.name,
    required this.document,
    required this.email,
    required this.phone,
    required this.whatsapp,
    required this.logo,
  });

  factory Barbershop.fromJson(Map<String, dynamic> json) {
    return Barbershop(
      id: json['id'] ?? '',
      name: json['name'] ?? '',
      document: json['document'] ?? '',
      email: json['email'] ?? '',
      phone: json['phone'] ?? '',
      whatsapp: json['whatsapp'] ?? '',
      logo: json['logo'] ?? '',
    );
  }
}
