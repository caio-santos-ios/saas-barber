import 'dart:convert';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';
import 'package:moment_dart/moment_dart.dart';

@pragma('vm:entry-point')
void startCallback() {
  FlutterForegroundTask.setTaskHandler(GenericTaskHandler());
}

class GenericTaskHandler extends TaskHandler {
  @override
  Future<void> onStart(DateTime timestamp, TaskStarter starter) async {}

  @override
  void onRepeatEvent(DateTime timestamp) async {
    final String? raw = await FlutterForegroundTask.getData(key: 'taskSetting');
    if (raw == null) return;

    final config = ForegroundTaskSetting.fromJson(jsonDecode(raw));

    final int minutes = timestamp.difference(config.startedAt).inMinutes;
    final int hours = timestamp.difference(config.startedAt).inHours;
    final int days = timestamp.difference(config.startedAt).inDays;
    final int weeks = timestamp.difference(config.startedAt).inWeeks;
    final int months = timestamp.difference(config.startedAt).inMonths;

    String messageTime = "$minutes m";

    if (hours >= 1) {
      messageTime = hours == 1 ? "1 hora" : "$hours horas";
    }
    if (days >= 1) {
      messageTime = days == 1 ? "1 dia" : "$days dias";
    }
    if (weeks >= 1) {
      messageTime = weeks == 1 ? "1 semana" : "$weeks semanas";
    }
    if (months >= 1) {
      messageTime = months == 1 ? "1 mês" : "$months meses";
    }

    FlutterForegroundTask.updateService(
      notificationTitle: config.title,
      notificationText:
          '${config.text} • $messageTime',
    );
  }

  @override
  Future<void> onDestroy(DateTime timestamp, bool isTimeout) async {}

  @override
  void onNotificationPressed() {
    FlutterForegroundTask.launchApp();
  }
}

class ForegroundTaskSetting {
  final String taskType; 
  final String taskId; 
  final String title; 
  final String text;
  final DateTime startedAt;

  ForegroundTaskSetting({
    required this.taskType,
    required this.taskId,
    required this.title,
    required this.text,
    required this.startedAt,
  });

  Map<String, dynamic> toJson() => {
    'taskType': taskType,
    'taskId': taskId,
    'title': title,
    'text': text,
    'startedAt': startedAt.toIso8601String(),
  };

  factory ForegroundTaskSetting.fromJson(Map<String, dynamic> json) =>
      ForegroundTaskSetting(
        taskType: json['taskType'],
        taskId: json['taskId'],
        title: json['title'],
        text: json['text'],
        startedAt: DateTime.parse(json['startedAt']),
      );
}

class ForegroundTaskService {
  void init() {
    FlutterForegroundTask.init(
      androidNotificationOptions: AndroidNotificationOptions(
        channelId: 'tarefa_ativa_channel',
        channelName: 'Tarefa em andamento',
        channelDescription: 'Notificação exibida enquanto uma tarefa está ativa',
        onlyAlertOnce: true,
      ),
      iosNotificationOptions: const IOSNotificationOptions(
        showNotification: true,
        playSound: false,
      ),
      foregroundTaskOptions: ForegroundTaskOptions(
        eventAction: ForegroundTaskEventAction.repeat(5000),
        autoRunOnBoot: false,
        allowWakeLock: true,
        allowWifiLock: true,
      ),
    );
  }

  Future<void> start({
    required String taskType,
    required String taskId,
    required String title,
    required String text,
    required DateTime startedAt
  }) async {
    final permission = await FlutterForegroundTask.checkNotificationPermission();
    if (permission != NotificationPermission.granted) {
      await FlutterForegroundTask.requestNotificationPermission();
    }

    final config = ForegroundTaskSetting(
      taskType: taskType,
      taskId: taskId,
      title: title,
      text: text,
      startedAt: startedAt
    );

    await FlutterForegroundTask.saveData(
      key: 'taskSetting',
      value: jsonEncode(config.toJson()),
    );

    await FlutterForegroundTask.startService(
      notificationTitle: title,
      notificationText: text,
      notificationIcon: const NotificationIcon(
        metaDataName: 'com.smartv4.app_smart.service.HEART_ICON',
      ),
      callback: startCallback,
    );
  }

  Future<void> stop() async {
    await FlutterForegroundTask.stopService();
    await FlutterForegroundTask.removeData(key: 'taskSetting');
  }
}