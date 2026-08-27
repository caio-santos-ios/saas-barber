class ServiceType {
  final String id;
  final String name;
  final String description;
  final int duration;
  final int durationMinutes;
  final double value;

  ServiceType({
    required this.id,
    required this.name,
    required this.description,
    required this.duration,
    required this.durationMinutes,
    required this.value,
  });

  factory ServiceType.fromJson(Map<String, dynamic> json) {
    final valRaw = json['value'] ?? json['price'] ?? 0;
    double val = 0.0;
    if (valRaw is num) {
      val = valRaw.toDouble();
    } else if (valRaw is String) {
      val = double.tryParse(valRaw.replaceAll(RegExp(r'[^0-9\.]'), '')) ?? 0.0;
    }

    final dur = json['duration'] ?? json['durationMinutes'] ?? json['duration_minutes'] ?? 0;
    final durInt = dur is int ? dur : (int.tryParse(dur.toString()) ?? 0);

    return ServiceType(
      id: json['id'] ?? '',
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      duration: durInt,
      durationMinutes: durInt,
      value: val,
    );
  }
}
