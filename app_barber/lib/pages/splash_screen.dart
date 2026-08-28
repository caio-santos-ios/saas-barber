import 'package:app_barber/pages/auth/login_page.dart';
import 'package:app_barber/pages/auth/select_barbershop_page.dart';
import 'package:app_barber/pages/homes/admin_home.dart';
import 'package:app_barber/pages/homes/barber_home.dart';
import 'package:app_barber/pages/homes/customer_home.dart';
import 'package:flutter/material.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'package:local_auth/local_auth.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  final LocalAuthentication _localAuth = LocalAuthentication();

  @override
  void initState() {
    super.initState();
    _checkAuth();
  }

  void _navigateToHome(String role) {
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
  }

  Future<void> _checkAuth() async {
    await Future.delayed(const Duration(seconds: 2));

    final authBox = Hive.box('auth');
    final String token = authBox.get('token') ?? "";
    final String role = authBox.get('role') ?? "";
    final bool useBiometrics = authBox.get('useBiometrics', defaultValue: false);
    final String barbershopId = authBox.get('barbershopId') ?? "";

    if (!mounted) return;

    if (token.isNotEmpty && role.isNotEmpty) {
      if (useBiometrics) {
        try {
          final canCheck = await _localAuth.canCheckBiometrics;
          final isSupported = await _localAuth.isDeviceSupported();

          if (canCheck || isSupported) {
            final bool didAuthenticate = await _localAuth.authenticate(
              localizedReason: 'Autentique-se para acessar o Na Régua',
            );

            if (!mounted) return;

            if (didAuthenticate) {
              _navigateToHome(role);
              return;
            } else {
              Navigator.of(context).pushReplacement(
                  MaterialPageRoute(builder: (_) => const LoginPage()));
              return;
            }
          }
        } catch (_) {
          if (!mounted) return;
          Navigator.of(context).pushReplacement(
              MaterialPageRoute(builder: (_) => const LoginPage()));
          return;
        }
      }

      _navigateToHome(role);
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
