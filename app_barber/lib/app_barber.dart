import 'package:app_barber/providers/theme_provider.dart';
import 'package:app_barber/pages/splash_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class AppBarber extends ConsumerWidget {
  AppBarber({super.key});

  final Color bgLightColor = Color(0xFFF8F9FA);
  final Color surfaceLightColor = Color(0xFFFFFFFF);
  final Color elevatedLightColor = Color(0xFFE9ECEF);
  final Color borderLightColor = Color(0xFFDEE2E6);

  final Color bgDarkColor = Color(0xFF121212);
  final Color surfaceDarkColor = Color(0xFF1E1E1E);
  final Color elevatedDarkColor = Color(0xFF2C2C2C);
  final Color borderDarkColor = Color(0xFF3A3A3A);

  final Color primaryColor = Color(0xFFC8923E);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final String themeMode = ref.watch(themeModeProvider("theme_mode"));

    return MaterialApp(
      title: 'Cortaê',
      theme: ThemeData(
        brightness: Brightness.light,
        colorScheme: ColorScheme.fromSeed(
          seedColor: primaryColor,
          primary: primaryColor,
          surface: surfaceLightColor,
          brightness: Brightness.light,
        ),
        scaffoldBackgroundColor: bgLightColor,
        cardColor: surfaceLightColor,
        dividerColor: borderLightColor,
        iconTheme: IconThemeData(color: Colors.black),
        appBarTheme: AppBarTheme(
          backgroundColor: primaryColor,
          iconTheme: IconThemeData(color: Colors.white),
          titleTextStyle: TextStyle(color: Colors.white, fontSize: 20),
        ),
        drawerTheme: DrawerThemeData(backgroundColor: bgLightColor),
        textTheme: TextTheme(
          titleLarge: TextStyle(
            fontSize: 25,
            fontWeight: FontWeight(700),
            color: Colors.black,
          ),
          titleMedium: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight(700),
            color: Colors.black,
          ),
          titleSmall: TextStyle(
            fontSize: 15,
            fontWeight: FontWeight(700),
            color: Colors.black,
          ),
          labelLarge: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight(600),
            color: Colors.black,
          ),
          labelMedium: TextStyle(
            fontSize: 15,
            fontWeight: FontWeight(600),
            color: Colors.black,
          ),
          labelSmall: TextStyle(
            fontSize: 12,
            fontWeight: FontWeight(600),
            color: Colors.black,
          ),
          bodyLarge: TextStyle(fontSize: 18, color: Colors.black),
          bodyMedium: TextStyle(fontSize: 15, color: Colors.black),
          bodySmall: TextStyle(fontSize: 12, color: Colors.black),
        ),
      ),
      darkTheme: ThemeData(
        brightness: Brightness.dark,
        colorScheme: ColorScheme.fromSeed(
          seedColor: primaryColor,
          primary: primaryColor,
          surface: surfaceDarkColor,
          brightness: Brightness.dark,
        ),
        scaffoldBackgroundColor: bgDarkColor,
        cardColor: surfaceDarkColor,
        dividerColor: borderDarkColor,
        iconTheme: IconThemeData(color: Colors.white),
        appBarTheme: AppBarTheme(
          backgroundColor: primaryColor,
          iconTheme: IconThemeData(color: Colors.white),
          titleTextStyle: TextStyle(color: Colors.white, fontSize: 20),
        ),
        drawerTheme: DrawerThemeData(backgroundColor: bgDarkColor),
        textTheme: TextTheme(
          titleLarge: TextStyle(
            fontSize: 25,
            fontWeight: FontWeight(700),
            color: Colors.white,
          ),
          titleMedium: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight(700),
            color: Colors.white,
          ),
          titleSmall: TextStyle(
            fontSize: 15,
            fontWeight: FontWeight(700),
            color: Colors.white,
          ),
          labelLarge: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight(600),
            color: Colors.white,
          ),
          labelMedium: TextStyle(
            fontSize: 15,
            fontWeight: FontWeight(600),
            color: Colors.white,
          ),
          labelSmall: TextStyle(
            fontSize: 12,
            fontWeight: FontWeight(600),
            color: Colors.white,
          ),
          bodyLarge: TextStyle(fontSize: 18, color: Colors.white),
          bodyMedium: TextStyle(fontSize: 15, color: Colors.white),
          bodySmall: TextStyle(fontSize: 12, color: Colors.white),
        ),
      ),
      themeMode: themeMode == "dark" ? ThemeMode.dark : ThemeMode.light,
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: [Locale('pt', 'BR')],
      locale: Locale('pt', 'BR'),
      debugShowCheckedModeBanner: false,
      home: const SplashScreen(),
    );
  }
}
