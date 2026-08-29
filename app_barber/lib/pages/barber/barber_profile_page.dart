import 'dart:convert';
import 'package:app_barber/pages/services/barber_service.dart';
import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hive/hive.dart';
import 'package:image_picker/image_picker.dart';
import 'package:app_barber/pages/auth/login_page.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/user.dart';
import 'package:app_barber/providers/theme_provider.dart';
import 'package:brasil_fields/brasil_fields.dart';
import 'package:flutter/services.dart';

class BarberProfilePage extends ConsumerStatefulWidget {
  const BarberProfilePage({super.key});

  @override
  ConsumerState<BarberProfilePage> createState() => _BarberProfilePageState();
}

class _BarberProfilePageState extends ConsumerState<BarberProfilePage> {
  final BarberService barberService = BarberService();

  final ApiClient _apiClient = ApiClient();
  bool _isLoading = true;
  bool _isSaving = false;

  final TextEditingController _nameController = TextEditingController();
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _whatsappController = TextEditingController();

  String _photo = '';
  bool _useBiometrics = false;

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  void _loadProfile() async {
    final authBox = Hive.box('auth');
    _photo = authBox.get('photo', defaultValue: '');
    _useBiometrics = authBox.get('useBiometrics', defaultValue: false) ||
        Hive.box('settings').get('biometrics', defaultValue: false);

    try {
      final response = await _apiClient.dio.get(
        '/users/${barberService.getUserId()}',
      );

      if (response.statusCode == 200 && response.data['data'] != null) {
        final user = User.fromJson(response.data['data']);
        _nameController.text = user.name;
        _emailController.text = user.email;
        _whatsappController.text = user.whatsapp;
        if (user.photo.isNotEmpty) {
          _photo = user.photo;
          await authBox.put('photo', _photo);
        }
        if (user.name.isNotEmpty) {
          await authBox.put('name', user.name);
        }
      }
    } catch (e) {}

    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _saveProfile() async {
    setState(() => _isSaving = true);
    try {
      Object body = {
        'id': barberService.getUserId(),
        'name': _nameController.text.trim(),
        'email': _emailController.text.trim(),
        'whatsapp': _whatsappController.text.trim(),
        'photo': _photo,
        'barbershopId': barberService.getBarbershopId()
      };

      final response = await _apiClient.dio.put(
        '/users',
        data: body,
      );

      if (response.statusCode == 200) {
        final authBox = Hive.box('auth');
        await authBox.put('photo', _photo);
        await authBox.put('name', _nameController.text.trim());
        if (mounted)
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Perfil atualizado com sucesso!'),
              backgroundColor: Colors.green,
            ),
          );
      } else {
        if (mounted)
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Erro ao atualizar perfil: ${response.statusCode}'),
              backgroundColor: Colors.red,
            ),
          );
      }
    } on DioException catch (e) {
      if (mounted)
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Erro de conexão ao atualizar: $e'),
            backgroundColor: Colors.red,
          ),
        );
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  Future<void> _showImageSourceDialog() async {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (context) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 20, horizontal: 16),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text(
                'Foto de Perfil',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              ListTile(
                leading: const Icon(Icons.camera_alt, color: Colors.blue),
                title: const Text('Tirar Foto (Câmera)'),
                onTap: () {
                  Navigator.pop(context);
                  _pickImage(ImageSource.camera);
                },
              ),
              ListTile(
                leading: const Icon(Icons.photo_library, color: Colors.green),
                title: const Text('Escolher da Galeria'),
                onTap: () {
                  Navigator.pop(context);
                  _pickImage(ImageSource.gallery);
                },
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _pickImage(ImageSource source) async {
    final picker = ImagePicker();
    try {
      final pickedFile = await picker.pickImage(
        source: source,
        maxWidth: 512,
        maxHeight: 512,
        imageQuality: 70,
      );

      if (pickedFile != null) {
        final bytes = await pickedFile.readAsBytes();
        final base64String = base64Encode(bytes);
        setState(() {
          _photo = 'data:image/jpeg;base64,$base64String';
        });

        await _saveProfile();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Erro ao selecionar imagem: $e'),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  ImageProvider? _getProfileImage() {
    if (_photo.isEmpty) return null;
    if (_photo.startsWith('http')) return NetworkImage(_photo);
    if (_photo.startsWith('data:image')) {
      try {
        final base64Str = _photo.split(',').last;
        return MemoryImage(base64Decode(base64Str));
      } catch (e) {
        return null;
      }
    }
    return null;
  }

  Future<void> _logout() async {
    final authBox = Hive.box('auth');
    await authBox.delete('token');
    await authBox.delete('refreshToken');
    await authBox.delete('role');
    await authBox.delete('photo');

    if (mounted) {
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute(builder: (_) => const LoginPage()),
        (route) => false,
      );
    }
  }

  void _showChangePasswordDialog() {
    final TextEditingController passwordController = TextEditingController();
    bool isLoading = false;

    showDialog(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Alterar Senha'),
              content: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Text(
                    'Digite a sua nova senha abaixo (mínimo de 6 caracteres).',
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: passwordController,
                    obscureText: true,
                    decoration: InputDecoration(
                      labelText: 'Nova Senha',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      filled: true,
                      fillColor: Theme.of(context).cardColor,
                    ),
                  ),
                ],
              ),
              actions: [
                TextButton(
                  onPressed: isLoading ? null : () => Navigator.pop(context),
                  child: const Text('Cancelar'),
                ),
                ElevatedButton(
                  onPressed: isLoading
                      ? null
                      : () async {
                          if (passwordController.text.length < 6) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(
                                content: Text(
                                  'A senha deve ter pelo menos 6 caracteres.',
                                ),
                                backgroundColor: Colors.orange,
                              ),
                            );
                            return;
                          }

                          setDialogState(() => isLoading = true);
                          try {
                            final response = await _apiClient.dio.post(
                              '/auth/update-password',
                              data: {'password': passwordController.text},
                            );

                            if (response.statusCode == 200) {
                              if (mounted) {
                                Navigator.pop(context);
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(
                                    content: Text(
                                      'Senha atualizada com sucesso!',
                                    ),
                                    backgroundColor: Colors.green,
                                  ),
                                );
                              }
                            } else {
                              if (mounted)
                                ScaffoldMessenger.of(context).showSnackBar(
                                  SnackBar(
                                    content: Text(
                                      'Erro ao atualizar senha: ${response.statusCode}',
                                    ),
                                    backgroundColor: Colors.red,
                                  ),
                                );
                            }
                          } catch (e) {
                            if (mounted)
                              ScaffoldMessenger.of(context).showSnackBar(
                                const SnackBar(
                                  content: Text(
                                    'Erro de conexão ao atualizar senha.',
                                  ),
                                  backgroundColor: Colors.red,
                                ),
                              );
                          } finally {
                            if (mounted)
                              setDialogState(() => isLoading = false);
                          }
                        },
                  child: isLoading
                      ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Salvar'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'Perfil do Profissional',
          style: TextStyle(
            color: Theme.of(context).textTheme.titleLarge?.color,
          ),
        ),
        centerTitle: true,
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: IconThemeData(color: Theme.of(context).iconTheme.color),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(24.0),
              child: Column(
                children: [
                  Center(
                    child: GestureDetector(
                      onTap: _showImageSourceDialog,
                      child: Stack(
                        children: [
                          CircleAvatar(
                            radius: 50,
                            backgroundColor: Theme.of(context).dividerColor,
                            backgroundImage: _getProfileImage(),
                            child: _photo.isEmpty
                                ? Icon(
                                    Icons.person,
                                    size: 50,
                                    color: Theme.of(context).iconTheme.color,
                                  )
                                : null,
                          ),
                          Positioned(
                            bottom: 0,
                            right: 0,
                            child: Container(
                              padding: const EdgeInsets.all(4),
                              decoration: const BoxDecoration(
                                color: Colors.black,
                                shape: BoxShape.circle,
                              ),
                              child: const Icon(
                                Icons.camera_alt,
                                color: Colors.white,
                                size: 20,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 32),
                  _buildTextField('Nome', _nameController),
                  const SizedBox(height: 16),
                  _buildTextField(
                    'E-mail',
                    _emailController,
                    keyboardType: TextInputType.emailAddress,
                  ),
                  const SizedBox(height: 16),
                  _buildTextField(
                    'WhatsApp',
                    _whatsappController,
                    keyboardType: TextInputType.phone,
                    inputFormatters: [
                      FilteringTextInputFormatter.digitsOnly,
                      TelefoneInputFormatter(),
                    ],
                  ),
                  const SizedBox(height: 24),
                  ElevatedButton(
                    onPressed: _isSaving ? null : _saveProfile,
                    style: ElevatedButton.styleFrom(
                      minimumSize: const Size(double.infinity, 50),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                    child: _isSaving
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(
                              color: Colors.white,
                              strokeWidth: 2,
                            ),
                          )
                        : const Text('Salvar Alterações'),
                  ),
                  const SizedBox(height: 32),
                  Material(
                    color: Theme.of(context).cardColor,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                      side: BorderSide(color: Theme.of(context).dividerColor),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      child: Consumer(
                        builder: (context, ref, child) {
                          final themeMode = ref.watch(
                            themeModeProvider("theme_mode"),
                          );
                          return SwitchListTile(
                            contentPadding: EdgeInsets.zero,
                            title: const Text(
                              'Tema Escuro',
                              style: TextStyle(fontWeight: FontWeight.w500),
                            ),
                            subtitle: const Text(
                              'Alternar entre claro e escuro',
                              style: TextStyle(
                                fontSize: 12,
                                color: Colors.grey,
                              ),
                            ),
                            value: themeMode == "dark",
                            onChanged: (val) {
                              final newTheme = val ? "dark" : "light";
                              ref
                                      .read(
                                        themeModeProvider(
                                          "theme_mode",
                                        ).notifier,
                                      )
                                      .state =
                                  newTheme;
                              Hive.box(
                                'settings',
                              ).put('theme_theme_mode', newTheme);
                            },
                            activeColor: Theme.of(context).colorScheme.primary,
                          );
                        },
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Material(
                    color: Theme.of(context).cardColor,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                      side: BorderSide(color: Theme.of(context).dividerColor),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      child: SwitchListTile(
                        contentPadding: EdgeInsets.zero,
                        title: const Text(
                          'Autenticação Biométrica',
                          style: TextStyle(fontWeight: FontWeight.w500),
                        ),
                        subtitle: const Text(
                          'Face ID / Touch ID',
                          style: TextStyle(fontSize: 12, color: Colors.grey),
                        ),
                        value: _useBiometrics,
                        onChanged: (val) {
                          setState(() => _useBiometrics = val);
                          Hive.box('settings').put('biometrics', val);
                          Hive.box('auth').put('useBiometrics', val);
                        },
                        activeColor: Theme.of(context).colorScheme.primary,
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton.icon(
                      onPressed: _showChangePasswordDialog,
                      icon: const Icon(Icons.lock_outline),
                      label: const Text('Alterar Senha'),
                      style: OutlinedButton.styleFrom(
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                        side: BorderSide(color: Theme.of(context).dividerColor),
                      ),
                    ),
                  ),
                  const SizedBox(height: 48),
                  ElevatedButton(
                    onPressed: _logout,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.red[50],
                      foregroundColor: Colors.red,
                      minimumSize: const Size(double.infinity, 50),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      elevation: 0,
                    ),
                    child: const Text(
                      'Sair do Aplicativo',
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                  ),
                  const SizedBox(height: 100),
                ],
              ),
            ),
    );
  }

  Widget _buildTextField(
    String label,
    TextEditingController controller, {
    TextInputType? keyboardType,
    List<TextInputFormatter>? inputFormatters,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: const TextStyle(
            color: Colors.grey,
            fontSize: 12,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 8),
        TextFormField(
          controller: controller,
          keyboardType: keyboardType,
          inputFormatters: inputFormatters,
          decoration: InputDecoration(
            filled: true,
            fillColor: Theme.of(context).cardColor,
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(color: Theme.of(context).dividerColor),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: BorderSide(color: Theme.of(context).dividerColor),
            ),
            contentPadding: const EdgeInsets.symmetric(
              horizontal: 16,
              vertical: 16,
            ),
          ),
        ),
      ],
    );
  }
}
