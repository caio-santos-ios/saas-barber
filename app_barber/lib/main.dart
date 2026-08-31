import 'package:app_barber/app_barber.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hive_flutter/hive_flutter.dart';

import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'firebase_options.dart';
import 'package:app_barber/services/notification_service.dart';
import 'package:app_barber/services/foreground_task_service.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';

@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  try {
    await Firebase.initializeApp(options: DefaultFirebaseOptions.currentPlatform);
  } catch (_) {}
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  try {
    await Firebase.initializeApp(
      options: DefaultFirebaseOptions.currentPlatform,
    );
    FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);
    FlutterForegroundTask.initCommunicationPort();
    ForegroundTaskService().init();

    await NotificationService().init();
  } catch (_) {}

  await Hive.initFlutter();
  
  await Hive.openBox('settings');
  await Hive.openBox('auth');

  runApp(
    ProviderScope(
      child: AppBarber(),
    ),
  );
}