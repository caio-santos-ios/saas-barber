class Appointment {
  final String id;
  final DateTime date;
  final String hour;
  final String notes;
  final String cancelNotes;
  final int status;
  final String barberId;
  final String barberName;
  final String customerId;
  final String customerName;
  final String serviceId;
  final String serviceTypeId;
  final String serviceTypeName;
  final String barbershopId;
  final double value;
  final String paymentStatus;
  final DateTime? startedAt;
  final DateTime? finishedAt;
  final int? rating;
  final String ratingComment;
  final String barberPhoto;
  final String customerPhoto;
  final String barberPhone;

  Appointment({
    required this.id,
    required this.date,
    required this.hour,
    required this.notes,
    required this.cancelNotes,
    required this.status,
    required this.barberId,
    required this.barberName,
    required this.customerId,
    required this.customerName,
    required this.serviceId,
    required this.serviceTypeId,
    required this.serviceTypeName,
    required this.barbershopId,
    required this.value,
    required this.paymentStatus,
    this.startedAt,
    this.finishedAt,
    this.rating,
    this.ratingComment = '',
    this.barberPhoto = '',
    this.customerPhoto = '',
    this.barberPhone = '',
  });

  static int _parseStatus(dynamic status) {
    if (status is int) return status;
    if (status is String) {
      switch (status.toLowerCase()) {
        case 'marcado': return 0;
        case 'cancelado': return 1;
        case 'finalizado': return 2;
        case 'naorealizado':
        case 'não realizado':
        case 'nao realizado': return 3;
        case 'emandamento':
        case 'em andamento': return 4;
        default:
          final parsed = int.tryParse(status);
          if (parsed != null) return parsed;
          return 0;
      }
    }
    return 0;
  }

  factory Appointment.fromJson(Map<String, dynamic> json) {
    final valRaw = json['value'] ?? json['price'] ?? 0;
    double val = 0.0;
    if (valRaw is num) {
      val = valRaw.toDouble();
    } else if (valRaw is String) {
      val = double.tryParse(valRaw.replaceAll(RegExp(r'[^0-9\.]'), '')) ?? 0.0;
    }

    DateTime? sAt;
    if (json['startedAt'] != null || json['started_at'] != null) {
      try {
        sAt = DateTime.parse(json['startedAt'] ?? json['started_at']);
      } catch (_) {}
    }

    DateTime? fAt;
    if (json['finishedAt'] != null || json['finished_at'] != null) {
      try {
        fAt = DateTime.parse(json['finishedAt'] ?? json['finished_at']);
      } catch (_) {}
    }

    int? rat;
    final rRaw = json['rating'];
    if (rRaw is int) {
      rat = rRaw;
    } else if (rRaw is num) {
      rat = rRaw.toInt();
    } else if (rRaw is String) {
      rat = int.tryParse(rRaw);
    }

    return Appointment(
      id: json['id'] ?? '',
      date: json['date'] != null ? DateTime.parse(json['date']) : DateTime.now(),
      hour: json['hour'] ?? '',
      notes: json['notes'] ?? '',
      cancelNotes: json['cancelNotes'] ?? json['cancel_notes'] ?? '',
      status: _parseStatus(json['status']),
      barberId: json['barberId'] ?? json['barber_id'] ?? '',
      barberName: json['barberName'] ?? json['barber_name'] ?? '',
      customerId: json['customerId'] ?? json['customer_id'] ?? '',
      customerName: json['customerName'] ?? json['customer_name'] ?? '',
      serviceId: json['serviceId'] ?? json['service_id'] ?? '',
      serviceTypeId: json['serviceTypeId'] ?? json['service_type_id'] ?? '',
      serviceTypeName: json['serviceTypeName'] ?? json['service_type_name'] ?? json['serviceName'] ?? json['service_name'] ?? '',
      barbershopId: json['barbershopId'] ?? json['barbershop_id'] ?? '',
      value: val,
      paymentStatus: json['paymentStatus'] ?? json['payment_status'] ?? '',
      startedAt: sAt,
      finishedAt: fAt,
      rating: rat,
      ratingComment: json['ratingComment'] ?? json['rating_comment'] ?? '',
      barberPhoto: json['barberPhoto'] ?? json['barber_photo'] ?? '',
      customerPhoto: json['customerPhoto'] ?? json['customer_photo'] ?? '',
      barberPhone: json['barberPhone'] ?? json['barber_phone'] ?? json['barberWhatsapp'] ?? '',
    );
  }
}

class CreateAppointmentRequest {
  final DateTime date;
  final String hour;
  final String notes;
  final String barberId;
  final String barberName;
  final String customerId;
  final String serviceId;
  final String serviceTypeId;
  final String customerName;
  final String serviceTypeName;
  final double value;

  CreateAppointmentRequest({
    required this.date,
    required this.hour,
    required this.notes,
    required this.barberId,
    required this.barberName,
    required this.customerId,
    required this.serviceId,
    required this.serviceTypeId,
    required this.customerName,
    required this.serviceTypeName,
    required this.value,
  });

  Map<String, dynamic> toJson() {
    return {
      'date': date.toIso8601String(),
      'hour': hour,
      'notes': notes,
      'barberId': barberId,
      'barberName': barberName,
      'customerId': customerId,
      'serviceId': serviceId,
      'serviceTypeId': serviceTypeId,
      'customerName': customerName,
      'serviceTypeName': serviceTypeName,
      'value': value,
    };
  }
}
