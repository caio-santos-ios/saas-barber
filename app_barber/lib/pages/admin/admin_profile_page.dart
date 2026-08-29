import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hive/hive.dart';
import 'package:image_picker/image_picker.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/user.dart';
import 'package:app_barber/pages/auth/login_page.dart';
import 'package:app_barber/pages/services/barber_service.dart';
import 'package:app_barber/providers/theme_provider.dart';
import 'package:brasil_fields/brasil_fields.dart';

class AdminProfilePage extends ConsumerStatefulWidget {
  const AdminProfilePage({super.key});

  @override
  ConsumerState<AdminProfilePage> createState() => _AdminProfilePageState();
}

class _AdminProfilePageState extends ConsumerState<AdminProfilePage> {
  final BarberService _barberService = BarberService();
  final ApiClient _apiClient = ApiClient();

  bool _isLoading = true;
  bool _isSaving = false;
  bool _useBiometrics = false;
  String _photo = '';

  final _nameCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  final _whatsCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    _loadProfile();
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _emailCtrl.dispose();
    _whatsCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadProfile() async {
    setState(() => _isLoading = true);
    final authBox = Hive.box('auth');
    _photo = authBox.get('photo', defaultValue: '');
    _useBiometrics = authBox.get('useBiometrics', defaultValue: false) ||
        Hive.box('settings').get('biometrics', defaultValue: false);

    try {
      final response = await _apiClient.dio.get('/users/${_barberService.getUserId()}');
      if (response.statusCode == 200 && response.data['data'] != null) {
        final user = User.fromJson(response.data['data']);
        _nameCtrl.text = user.name;
        _emailCtrl.text = user.email;
        _whatsCtrl.text = user.whatsapp;
        if (user.photo.isNotEmpty) {
          _photo = user.photo;
          await authBox.put('photo', _photo);
        }
        if (user.name.isNotEmpty) {
          await authBox.put('name', user.name);
        }
      }
    } catch (_) {}

    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _saveProfile() async {
    setState(() => _isSaving = true);
    try {
      final response = await _apiClient.dio.put('/users', data: {
        'id': _barberService.getUserId(),
        'name': _nameCtrl.text.trim(),
        'email': _emailCtrl.text.trim(),
        'whatsapp': _whatsCtrl.text.replaceAll(RegExp(r'\D'), ''),
        'photo': _photo,
        'barbershopId': _barberService.getBarbershopId()
      });
      if (response.statusCode == 200) {
        final authBox = Hive.box('auth');
        await authBox.put('photo', _photo);
        await authBox.put('name', _nameCtrl.text.trim());
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Perfil atualizado com sucesso!'), backgroundColor: Colors.green),
          );
        }
      } else {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('Erro ao atualizar perfil: ${response.statusCode}'), backgroundColor: Colors.red),
          );
        }
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Erro ao atualizar perfil.'), backgroundColor: Colors.red),
        );
      }
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
      final picked = await picker.pickImage(source: source, maxWidth: 512, maxHeight: 512, imageQuality: 70);
      if (picked != null) {
        final bytes = await picked.readAsBytes();
        final b64 = 'data:image/jpeg;base64,${base64Encode(bytes)}';
        setState(() => _photo = b64);
        await _saveProfile();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erro ao selecionar imagem: $e'), backgroundColor: Colors.red),
        );
      }
    }
  }

  ImageProvider? _getProfileImage() {
    if (_photo.isEmpty) return null;
    if (_photo.startsWith('http')) return NetworkImage(_photo);
    if (_photo.startsWith('data:image')) {
      try {
        return MemoryImage(base64Decode(_photo.split(',').last));
      } catch (_) {
        return null;
      }
    }
    return null;
  }

  void _showChangePasswordDialog() {
    final passCtrl = TextEditingController();
    bool isSaving = false;

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setD) => AlertDialog(
          title: const Text('Alterar Senha'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('Digite a nova senha (mínimo 6 caracteres).'),
              const SizedBox(height: 16),
              TextField(
                controller: passCtrl,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: 'Nova Senha',
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                  filled: true,
                  fillColor: Theme.of(context).cardColor,
                ),
              ),
            ],
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancelar')),
            ElevatedButton(
              onPressed: isSaving
                  ? null
                  : () async {
                      if (passCtrl.text.length < 6) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('Mínimo de 6 caracteres.'), backgroundColor: Colors.orange),
                        );
                        return;
                      }
                      setD(() => isSaving = true);
                      try {
                        await _apiClient.dio.post('/auth/update-password', data: {'password': passCtrl.text});
                        if (mounted) {
                          Navigator.pop(ctx);
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('Senha atualizada com sucesso!'), backgroundColor: Colors.green),
                          );
                        }
                      } catch (_) {
                        if (mounted) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('Erro ao atualizar senha.'), backgroundColor: Colors.red),
                          );
                        }
                      } finally {
                        if (mounted) setD(() => isSaving = false);
                      }
                    },
              child: isSaving
                  ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                  : const Text('Salvar'),
            ),
          ],
        ),
      ),
    );
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).scaffoldBackgroundColor,
      appBar: AppBar(
        title: Text('Meu Perfil', style: TextStyle(color: Theme.of(context).textTheme.titleLarge?.color)),
        centerTitle: true,
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: IconThemeData(color: Theme.of(context).iconTheme.color),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: Column(
                children: [
                  Center(
                    child: GestureDetector(
                      onTap: _showImageSourceDialog,
                      child: Stack(
                        children: [
                          CircleAvatar(
                            radius: 52,
                            backgroundColor: Theme.of(context).dividerColor,
                            backgroundImage: _getProfileImage(),
                            child: _photo.isEmpty
                                ? Icon(Icons.person, size: 52, color: Theme.of(context).iconTheme.color)
                                : null,
                          ),
                          Positioned(
                            bottom: 0,
                            right: 0,
                            child: Container(
                              padding: const EdgeInsets.all(4),
                              decoration: const BoxDecoration(color: Colors.black, shape: BoxShape.circle),
                              child: const Icon(Icons.camera_alt, color: Colors.white, size: 20),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 28),
                  _buildTextField('Nome', _nameCtrl),
                  const SizedBox(height: 12),
                  _buildTextField('E-mail', _emailCtrl, keyboardType: TextInputType.emailAddress),
                  const SizedBox(height: 12),
                  _buildTextField('WhatsApp', _whatsCtrl,
                      keyboardType: TextInputType.phone,
                      formatters: [FilteringTextInputFormatter.digitsOnly, TelefoneInputFormatter()]),
                  const SizedBox(height: 24),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _isSaving ? null : _saveProfile,
                      style: ElevatedButton.styleFrom(
                        minimumSize: const Size(double.infinity, 50),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      child: _isSaving
                          ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                          : const Text('Salvar Alterações'),
                    ),
                  ),
                  const SizedBox(height: 32),
                  Material(
                    color: Theme.of(context).cardColor,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                      side: BorderSide(color: Theme.of(context).dividerColor),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      child: Consumer(
                        builder: (context, ref, child) {
                          final themeMode = ref.watch(themeModeProvider("theme_mode"));
                          return SwitchListTile(
                            contentPadding: EdgeInsets.zero,
                            title: const Text('Tema Escuro', style: TextStyle(fontWeight: FontWeight.w500)),
                            subtitle: const Text('Alternar entre claro e escuro', style: TextStyle(fontSize: 12, color: Colors.grey)),
                            value: themeMode == "dark",
                            onChanged: (val) {
                              final newTheme = val ? "dark" : "light";
                              ref.read(themeModeProvider("theme_mode").notifier).state = newTheme;
                              Hive.box('settings').put('theme_theme_mode', newTheme);
                            },
                            activeColor: Theme.of(context).colorScheme.primary,
                          );
                        },
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),
                  Material(
                    color: Theme.of(context).cardColor,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                      side: BorderSide(color: Theme.of(context).dividerColor),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      child: SwitchListTile(
                        contentPadding: EdgeInsets.zero,
                        title: const Text('Autenticação Biométrica', style: TextStyle(fontWeight: FontWeight.w500)),
                        subtitle: const Text('Face ID / Touch ID', style: TextStyle(fontSize: 12, color: Colors.grey)),
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
                  const SizedBox(height: 12),
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton.icon(
                      onPressed: _showChangePasswordDialog,
                      icon: const Icon(Icons.lock_outline),
                      label: const Text('Alterar Senha'),
                      style: OutlinedButton.styleFrom(
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
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
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      elevation: 0,
                    ),
                    child: const Text('Sair do Aplicativo', style: TextStyle(fontWeight: FontWeight.bold)),
                  ),
                  const SizedBox(height: 40),
                ],
              ),
            ),
    );
  }

  Widget _buildTextField(
    String label,
    TextEditingController ctrl, {
    TextInputType? keyboardType,
    List<TextInputFormatter>? formatters,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: const TextStyle(color: Colors.grey, fontSize: 12, fontWeight: FontWeight.bold)),
        const SizedBox(height: 6),
        TextFormField(
          controller: ctrl,
          keyboardType: keyboardType,
          inputFormatters: formatters,
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
            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
          ),
        ),
      ],
    );
  }
}
