import 'package:app_barber/widgets/custom_text_field.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/auth.dart';
import 'package:app_barber/repositories/auth_repository.dart';
import 'package:app_barber/pages/homes/admin_home.dart';
import 'package:app_barber/pages/homes/barber_home.dart';
import 'package:app_barber/pages/homes/customer_home.dart';
import 'package:app_barber/pages/auth/register_page.dart';
import 'package:app_barber/pages/auth/select_barbershop_page.dart';
import 'package:app_barber/pages/auth/forgot_password_page.dart';
import 'package:app_barber/pages/auth/update_password_page.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:local_auth/local_auth.dart';
import 'package:hive/hive.dart';
import 'package:firebase_messaging/firebase_messaging.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  final LocalAuthentication _localAuth = LocalAuthentication();
  
  bool _obscurePassword = true;
  bool _isLoading = false;
  String _selectedRole = 'Customer';

  late final AuthRepository _authRepository;

  @override
  void initState() {
    super.initState();
    final apiClient = ApiClient();
    _authRepository = AuthRepository(apiClient);
    _loadSavedData();
  }

  void _loadSavedData() {
    final authBox = Hive.box('auth');
    final savedRole = authBox.get('savedRole', defaultValue: 'Customer') as String;
    if (savedRole.isNotEmpty) {
      _selectedRole = savedRole;
    }
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _checkAutoBiometrics();
    });
  }

  Future<void> _checkAutoBiometrics() async {
    final authBox = Hive.box('auth');
    final useBiometrics = (authBox.get('useBiometrics', defaultValue: false) as bool) ||
        (Hive.box('settings').get('biometrics', defaultValue: false) as bool);
    final savedPassword = authBox.get('savedPassword', defaultValue: '') as String;
    final savedEmail = authBox.get('savedEmail', defaultValue: '') as String;
    final savedRole = authBox.get('savedRole', defaultValue: _selectedRole) as String;

    if (useBiometrics && savedPassword.isNotEmpty && savedEmail.isNotEmpty) {
      try {
        final canCheck = await _localAuth.canCheckBiometrics;
        final isSupported = await _localAuth.isDeviceSupported();

        if (canCheck || isSupported) {
          final bool didAuthenticate = await _localAuth.authenticate(
            localizedReason: 'Por favor, confirme sua identidade para entrar',
          );

          if (didAuthenticate && mounted) {
            _passwordController.text = savedPassword;
            _emailController.text = savedEmail;
            _selectedRole = savedRole;
            _doLogin();
          }
        }
      } catch (e) {
        debugPrint('Biometric auth error: $e');
      }
    }
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  void _routeUser(bool passwordResetRequired, String role) {
    if (passwordResetRequired) {
      Navigator.of(context).pushReplacement(
          MaterialPageRoute(builder: (_) => const UpdatePasswordPage()));
      return;
    }
    
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

  Future<void> _askForBiometrics() async {
    final authBox = Hive.box('auth');
    final useBiometrics = authBox.get('useBiometrics');

    if (useBiometrics == null) {
      final canCheck = await _localAuth.canCheckBiometrics;
      final isSupported = await _localAuth.isDeviceSupported();

      if ((canCheck || isSupported) && mounted) {
        final result = await showDialog<bool>(
          context: context,
          barrierDismissible: false,
          builder: (context) => AlertDialog(
            title: const Text('Login Biométrico'),
            content: const Text('Deseja habilitar o Face ID / Touch ID para entrar mais rápido nas próximas vezes?'),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(false),
                child: const Text('Não'),
              ),
              ElevatedButton(
                onPressed: () => Navigator.of(context).pop(true),
                child: const Text('Sim, habilitar'),
              ),
            ],
          ),
        );
        if (result != null) {
          await authBox.put('useBiometrics', result);
          if (result == true) {
            await authBox.put('savedPassword', _passwordController.text);
            await authBox.put('savedEmail', _emailController.text.trim());
            await authBox.put('savedRole', _selectedRole);
          }
        }
      }
    } else if (useBiometrics == true) {
      await authBox.put('savedPassword', _passwordController.text);
      await authBox.put('savedEmail', _emailController.text.trim());
      await authBox.put('savedRole', _selectedRole);
    }
  }

  Future<void> _doLogin() async {
    if (_formKey.currentState!.validate()) {
      setState(() {
        _isLoading = true;
      });

      try {
        String? fcmToken;
        try {
          await FirebaseMessaging.instance.requestPermission();
          fcmToken = await FirebaseMessaging.instance.getToken();
        } catch (e) {
          debugPrint('Failed to get FCM token: $e');
        }

        final authBox = Hive.box('auth');
        final currentBarbershopId = authBox.get('barbershopId', defaultValue: '') as String;

        final request = LoginRequest(
          email: _emailController.text.trim(),
          password: _passwordController.text,
          tokenFCM: fcmToken,
          role: _selectedRole,
          barbershopId: currentBarbershopId,
        );

        final session = await _authRepository.login(request);

        if (!mounted) return;

        if (session != null) {
          await authBox.put('savedEmail', _emailController.text.trim());
          await authBox.put('savedRole', _selectedRole);
          await authBox.put('savedPassword', _passwordController.text);
          await _askForBiometrics();
          
          if (!mounted) return;
          _routeUser(session.passwordResetRequired, session.role);
        }
      } on DioException catch (e) {
        if (mounted) {
          final errorMsg = e.response?.data is Map 
              ? (e.response?.data["message"] ?? e.response?.data["error"] ?? 'E-mail ou senha inválidos.')
              : (e.response?.data?.toString() ?? 'E-mail ou senha inválidos.');
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(errorMsg.toString()),
              backgroundColor: Colors.red,
            ),
          );
        }
      } finally {
        if (mounted) {
          setState(() {
            _isLoading = false;
          });
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final authBox = Hive.box('auth');
    final barbershopName = authBox.get('barbershopName', defaultValue: '') as String;
    final primary = Theme.of(context).colorScheme.primary;

    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24.0),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Image.asset(
                    'assets/images/logo.png',
                    width: 120,
                    height: 120,
                    fit: BoxFit.contain,
                  ),
                  const SizedBox(height: 24),
                  if (barbershopName.isNotEmpty) ...[
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
                      decoration: BoxDecoration(
                        color: primary.withValues(alpha: 0.08),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: primary.withValues(alpha: 0.25),
                        ),
                      ),
                      child: Row(
                        children: [
                          Icon(
                            Icons.storefront_rounded,
                            color: primary,
                            size: 20,
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              barbershopName,
                              style: TextStyle(
                                fontWeight: FontWeight.bold,
                                color: primary,
                                fontSize: 14,
                              ),
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                          TextButton(
                            onPressed: () {
                              authBox.delete('barbershopId');
                              authBox.delete('barbershopName');
                              authBox.delete('barbershopLogo');
                              authBox.delete('barbershopCode');
                              Navigator.of(context).pushReplacement(
                                MaterialPageRoute(
                                  builder: (_) => const SelectBarbershopPage(),
                                ),
                              );
                            },
                            style: TextButton.styleFrom(
                              padding: const EdgeInsets.symmetric(horizontal: 8),
                              minimumSize: Size.zero,
                              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                            ),
                            child: const Text(
                              'Trocar',
                              style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 16),
                  ],
                  Container(
                    decoration: BoxDecoration(
                      color: Theme.of(context).brightness == Brightness.dark
                          ? const Color(0xFF2C2C2C)
                          : const Color(0xFFE9ECEF),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    padding: const EdgeInsets.all(4),
                    child: Row(
                      children: [
                        Expanded(
                          child: GestureDetector(
                            onTap: () => setState(() => _selectedRole = 'Customer'),
                            child: Container(
                              padding: const EdgeInsets.symmetric(vertical: 10),
                              decoration: BoxDecoration(
                                color: _selectedRole == 'Customer' ? primary : Colors.transparent,
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Text(
                                'Sou Cliente',
                                textAlign: TextAlign.center,
                                style: TextStyle(
                                  fontWeight: FontWeight.bold,
                                  fontSize: 14,
                                  color: _selectedRole == 'Customer'
                                      ? Colors.white
                                      : Theme.of(context).textTheme.bodyMedium?.color,
                                ),
                              ),
                            ),
                          ),
                        ),
                        Expanded(
                          child: GestureDetector(
                            onTap: () => setState(() => _selectedRole = 'Barber'),
                            child: Container(
                              padding: const EdgeInsets.symmetric(vertical: 10),
                              decoration: BoxDecoration(
                                color: _selectedRole == 'Barber' ? primary : Colors.transparent,
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Text(
                                'Sou Profissional',
                                textAlign: TextAlign.center,
                                style: TextStyle(
                                  fontWeight: FontWeight.bold,
                                  fontSize: 14,
                                  color: _selectedRole == 'Barber'
                                      ? Colors.white
                                      : Theme.of(context).textTheme.bodyMedium?.color,
                                ),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 20),
                  CustomTextField(
                    controller: _emailController,
                    labelText: 'E-mail',
                    prefixIcon: Icons.email,
                    keyboardType: TextInputType.emailAddress,
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'O e-mail é obrigatório';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),
                  CustomTextField(
                    controller: _passwordController,
                    labelText: 'Senha',
                    prefixIcon: Icons.lock,
                    obscureText: _obscurePassword,
                    suffixIcon: IconButton(
                      icon: Icon(
                        _obscurePassword ? Icons.visibility : Icons.visibility_off,
                      ),
                      onPressed: () {
                        setState(() {
                          _obscurePassword = !_obscurePassword;
                        });
                      },
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'A senha é obrigatória';
                      }
                      return null;
                    },
                  ),
                  Align(
                    alignment: Alignment.centerRight,
                    child: TextButton(
                      onPressed: () {
                        Navigator.of(context).push(
                          MaterialPageRoute(builder: (_) => const ForgotPasswordPage()),
                        );
                      },
                      child: Text(
                        'Esqueci minha senha',
                        style: TextStyle(
                          color: Theme.of(context).colorScheme.primary,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      Expanded(
                        child: SizedBox(
                          height: 56,
                          child: ElevatedButton(
                            onPressed: _isLoading ? null : _doLogin,
                            style: ElevatedButton.styleFrom(
                              backgroundColor: Theme.of(context).colorScheme.primary,
                              foregroundColor: Colors.white,
                              elevation: 0,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                            ),
                            child: _isLoading
                                ? const SizedBox(
                                    width: 24,
                                    height: 24,
                                    child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5),
                                  )
                                : const Text('Entrar', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                          ),
                        ),
                      ),
                      if (((authBox.get('useBiometrics', defaultValue: false) == true) || (Hive.box('settings').get('biometrics', defaultValue: false) == true)) && (authBox.get('savedPassword', defaultValue: '') as String).isNotEmpty) ...[
                        const SizedBox(width: 12),
                        SizedBox(
                          height: 56,
                          width: 56,
                          child: OutlinedButton(
                            onPressed: _isLoading ? null : _checkAutoBiometrics,
                            style: OutlinedButton.styleFrom(
                              padding: EdgeInsets.zero,
                              side: BorderSide(color: Theme.of(context).colorScheme.primary, width: 1.5),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                            ),
                            child: Icon(Icons.fingerprint, color: Theme.of(context).colorScheme.primary, size: 28),
                          ),
                        ),
                      ],
                    ],
                  ),
                  if (_selectedRole == 'Customer') ...[
                    const SizedBox(height: 24),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Text(
                          'Ainda não é cliente?',
                          style: TextStyle(color: Colors.grey),
                        ),
                        TextButton(
                          onPressed: () {
                            final authBox = Hive.box('auth');
                            final currentBarbershopId = authBox.get('barbershopId', defaultValue: '') as String;
                            Navigator.of(context).push(
                              MaterialPageRoute(builder: (_) => RegisterPage(barbershopId: currentBarbershopId)),
                            );
                          },
                          child: Text(
                            'Cadastre-se',
                            style: TextStyle(
                              color: Theme.of(context).colorScheme.primary,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
