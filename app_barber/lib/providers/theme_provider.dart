import 'package:flutter_riverpod/legacy.dart';
import 'package:hive/hive.dart';

final themeModeProvider = StateProvider.family<String, String>((ref, key) {
  try {
    return Hive.box('settings').get('theme_$key', defaultValue: 'light');
  } catch (e) {
    return 'light';
  }
});