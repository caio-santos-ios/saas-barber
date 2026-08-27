import 'package:app_barber/pages/auth/login_page.dart';
import 'package:app_barber/pages/auth/select_barbershop_page.dart';
import 'package:app_barber/pages/homes/admin_home.dart';
import 'package:app_barber/pages/homes/barber_home.dart';
import 'package:app_barber/pages/homes/customer_home.dart';
import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _checkAuth();
  }

  Future<void> _checkAuth() async {
    await Future.delayed(const Duration(seconds: 2));

    final authBox = Hive.box('auth');
    final String token = authBox.get('token') ?? "";
    final String role = authBox.get('role') ?? "";
    final String barbershopId = authBox.get('barbershopId') ?? "";

    if (!mounted) return;

    if (token.isNotEmpty && role.isNotEmpty) {
      if (role == 'Admin') {
        Navigator.of(context).pushReplacement(
            MaterialPageRoute(builder: (_) => const AdminHomePage()));
      } else if (role == 'Barber') {
        Navigator.of(context).pushReplacement(
            MaterialPageRoute(builder: (_) => const BarberHomePage()));
      } else {
        Navigator.of(context).pushReplacement(
            MaterialPageRoute(builder: (_) => const CustomerHomePage()));
      }
    } else {
      if (barbershopId.isNotEmpty) {
        Navigator.of(context).pushReplacement(
            MaterialPageRoute(builder: (_) => const LoginPage()));
      } else {
        Navigator.of(context).pushReplacement(
            MaterialPageRoute(builder: (_) => const SelectBarbershopPage()));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.primary,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Image.asset(
              'assets/images/logo.png',
              width: 150,
              height: 150,
              fit: BoxFit.contain,
            ),
            const SizedBox(height: 32),
            const CircularProgressIndicator(color: Colors.white),
          ],
        ),
      ),
    );
  }
}
