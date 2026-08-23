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
    return ServiceType(
      id: json['id'] ?? '',
      name: json['name'] ?? '',
      description: json['description'] ?? '',
      duration: json['duration'] ?? 0,
      durationMinutes: json['durationMinutes'] ?? json['duration_minutes'] ?? 0,
      value: (json['value'] ?? 0).toDouble(),
    );
  }
}
