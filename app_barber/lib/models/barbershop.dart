class BarbershopAddress {
  final String zipCode;
  final String street;
  final String number;
  final String complement;
  final String neighborhood;
  final String city;
  final String state;

  BarbershopAddress({
    this.zipCode = '',
    this.street = '',
    this.number = '',
    this.complement = '',
    this.neighborhood = '',
    this.city = '',
    this.state = '',
  });

  factory BarbershopAddress.fromJson(Map<String, dynamic> json) {
    return BarbershopAddress(
      zipCode: json['zipCode'] ?? json['ZipCode'] ?? '',
      street: json['street'] ?? json['Street'] ?? '',
      number: json['number'] ?? json['Number'] ?? '',
      complement: json['complement'] ?? json['Complement'] ?? '',
      neighborhood: json['neighborhood'] ?? json['Neighborhood'] ?? '',
      city: json['city'] ?? json['City'] ?? '',
      state: json['state'] ?? json['State'] ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
        'zipCode': zipCode,
        'street': street,
        'number': number,
        'complement': complement,
        'neighborhood': neighborhood,
        'city': city,
        'state': state,
        'country': 'Brasil',
      };
}

class Barbershop {
  final String id;
  final String code;
  final String name;
  final String document;
  final String email;
  final String phone;
  final String whatsapp;
  final String logo;
  final BarbershopAddress address;

  Barbershop({
    required this.id,
    this.code = '',
    required this.name,
    required this.document,
    required this.email,
    required this.phone,
    required this.whatsapp,
    required this.logo,
    BarbershopAddress? address,
  }) : address = address ?? BarbershopAddress();

  factory Barbershop.fromJson(Map<String, dynamic> json) {
    return Barbershop(
      id: json['id'] ?? '',
      code: json['code'] ?? '',
      name: json['name'] ?? '',
      document: json['document'] ?? '',
      email: json['email'] ?? '',
      phone: json['phone'] ?? json['Phone'] ?? '',
      whatsapp: json['whatsapp'] ?? '',
      logo: json['logo'] ?? '',
      address: json['address'] != null
          ? BarbershopAddress.fromJson(json['address'])
          : BarbershopAddress(),
    );
  }
}
