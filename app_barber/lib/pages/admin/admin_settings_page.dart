import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:hive/hive.dart';
import 'package:image_picker/image_picker.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/barbershop.dart';
import 'package:app_barber/models/user.dart';
import 'package:app_barber/pages/auth/login_page.dart';
import 'package:app_barber/pages/services/barber_service.dart';
import 'package:app_barber/providers/theme_provider.dart';
import 'package:app_barber/repositories/barbershop_repository.dart';
import 'package:brasil_fields/brasil_fields.dart';
import 'package:dio/dio.dart';
import 'package:http/http.dart' as http;

class AdminSettingsPage extends ConsumerStatefulWidget {
  const AdminSettingsPage({super.key});

  @override
  ConsumerState<AdminSettingsPage> createState() => _AdminSettingsPageState();
}

class _AdminSettingsPageState extends ConsumerState<AdminSettingsPage> {
  final BarberService _barberService = BarberService();
  final ApiClient _apiClient = ApiClient();
  late final BarbershopRepository _barbershopRepo;

  bool _isLoading = true;
  bool _isSavingShop = false;
  bool _isSavingProfile = false;
  bool _isSearchingCep = false;
  bool _useBiometrics = false;
  String _photo = '';

  final _shopNameCtrl = TextEditingController();
  final _shopDocCtrl = TextEditingController();
  final _shopPhoneCtrl = TextEditingController();
  final _shopZipCtrl = TextEditingController();
  final _shopStreetCtrl = TextEditingController();
  final _shopNumberCtrl = TextEditingController();
  final _shopComplementCtrl = TextEditingController();
  final _shopNeighborhoodCtrl = TextEditingController();
  final _shopCityCtrl = TextEditingController();
  final _shopStateCtrl = TextEditingController();

  final _profileNameCtrl = TextEditingController();
  final _profileEmailCtrl = TextEditingController();
  final _profileWhatsCtrl = TextEditingController();

  String _barbershopId = '';

  @override
  void initState() {
    super.initState();
    _barbershopRepo = BarbershopRepository(_apiClient);
    _loadAll();
  }

  @override
  void dispose() {
    _shopNameCtrl.dispose();
    _shopDocCtrl.dispose();
    _shopPhoneCtrl.dispose();
    _shopZipCtrl.dispose();
    _shopStreetCtrl.dispose();
    _shopNumberCtrl.dispose();
    _shopComplementCtrl.dispose();
    _shopNeighborhoodCtrl.dispose();
    _shopCityCtrl.dispose();
    _shopStateCtrl.dispose();
    _profileNameCtrl.dispose();
    _profileEmailCtrl.dispose();
    _profileWhatsCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadAll() async {
    setState(() => _isLoading = true);
    final authBox = Hive.box('auth');
    _barbershopId = authBox.get('barbershopId', defaultValue: '');
    _photo = authBox.get('photo', defaultValue: '');
    _useBiometrics = authBox.get('useBiometrics', defaultValue: false) ||
        Hive.box('settings').get('biometrics', defaultValue: false);

    try {
      final results = await Future.wait([
        _barbershopRepo.getBarbershop(_barbershopId),
        _apiClient.dio.get('/users/${_barberService.getUserId()}'),
      ]);

      final shop = results[0] as Barbershop?;
      final userResp = results[1] as Response;
      final user = User.fromJson(userResp.data['data']);

      if (shop != null) {
        _shopNameCtrl.text = shop.name;
        _shopDocCtrl.text = shop.document;
        _shopPhoneCtrl.text = shop.phone;
        _shopZipCtrl.text = shop.address.zipCode;
        _shopStreetCtrl.text = shop.address.street;
        _shopNumberCtrl.text = shop.address.number;
        _shopComplementCtrl.text = shop.address.complement;
        _shopNeighborhoodCtrl.text = shop.address.neighborhood;
        _shopCityCtrl.text = shop.address.city;
        _shopStateCtrl.text = shop.address.state;
      }

      _profileNameCtrl.text = user.name;
      _profileEmailCtrl.text = user.email;
      _profileWhatsCtrl.text = user.whatsapp;
    } catch (e) {
      // silently ignore
    }

    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _searchCep() async {
    final cep = _shopZipCtrl.text.replaceAll(RegExp(r'\D'), '');
    if (cep.length != 8) return;
    setState(() => _isSearchingCep = true);
    try {
      final response = await http.get(Uri.parse('https://viacep.com.br/ws/$cep/json/'));
      final data = json.decode(response.body);
      if (data['erro'] == null) {
        _shopStreetCtrl.text = data['logradouro'] ?? '';
        _shopNeighborhoodCtrl.text = data['bairro'] ?? '';
        _shopCityCtrl.text = data['localidade'] ?? '';
        _shopStateCtrl.text = data['uf'] ?? '';
      }
    } catch (e) {
      // ignore
    } finally {
      if (mounted) setState(() => _isSearchingCep = false);
    }
  }

  Future<void> _saveBarbershop() async {
    setState(() => _isSavingShop = true);
    final ok = await _barbershopRepo.updateBarbershop({
      'id': _barbershopId,
      'name': _shopNameCtrl.text,
      'document': _shopDocCtrl.text.replaceAll(RegExp(r'\D'), ''),
      'phone': _shopPhoneCtrl.text.replaceAll(RegExp(r'\D'), ''),
      'address': BarbershopAddress(
        zipCode: _shopZipCtrl.text.replaceAll(RegExp(r'\D'), ''),
        street: _shopStreetCtrl.text,
        number: _shopNumberCtrl.text,
        complement: _shopComplementCtrl.text,
        neighborhood: _shopNeighborhoodCtrl.text,
        city: _shopCityCtrl.text,
        state: _shopStateCtrl.text,
      ).toJson(),
    });
    if (mounted) {
      setState(() => _isSavingShop = false);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Text(ok ? 'Barbearia atualizada com sucesso!' : 'Erro ao salvar dados da barbearia.'),
        backgroundColor: ok ? Colors.green : Colors.red,
      ));
    }
  }

  Future<void> _saveProfile() async {
    setState(() => _isSavingProfile = true);
    try {
      await _apiClient.dio.put('/users', data: {
        'id': _barberService.getUserId(),
        'name': _profileNameCtrl.text,
        'email': _profileEmailCtrl.text,
        'whatsapp': _profileWhatsCtrl.text.replaceAll(RegExp(r'\D'), ''),
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Perfil atualizado com sucesso!'), backgroundColor: Colors.green),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Erro ao atualizar perfil.'), backgroundColor: Colors.red),
        );
      }
    } finally {
      if (mounted) setState(() => _isSavingProfile = false);
    }
  }

  Future<void> _pickImage() async {
    final picker = ImagePicker();
    try {
      final picked = await picker.pickImage(source: ImageSource.gallery, maxWidth: 512, maxHeight: 512, imageQuality: 70);
      if (picked != null) {
        final bytes = await picked.readAsBytes();
        final b64 = 'data:image/jpeg;base64,${base64Encode(bytes)}';
        setState(() => _photo = b64);
        await _saveProfile();
        Hive.box('auth').put('photo', _photo);
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
    await authBox.delete('useBiometrics');
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
        title: Text('Configurações', style: TextStyle(color: Theme.of(context).textTheme.titleLarge?.color)),
        centerTitle: true,
        backgroundColor: Colors.transparent,
        elevation: 0,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildSectionTitle('Dados da Barbearia'),
                  const SizedBox(height: 16),
                  _buildTextField('Nome da Barbearia', _shopNameCtrl),
                  const SizedBox(height: 12),
                  _buildTextField('CNPJ / CPF', _shopDocCtrl,
                      keyboardType: TextInputType.number,
                      formatters: [FilteringTextInputFormatter.digitsOnly, CnpjInputFormatter()]),
                  const SizedBox(height: 12),
                  _buildTextField('Telefone', _shopPhoneCtrl,
                      keyboardType: TextInputType.phone,
                      formatters: [FilteringTextInputFormatter.digitsOnly, TelefoneInputFormatter()]),
                  const SizedBox(height: 12),
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Expanded(
                        child: _buildTextField('CEP', _shopZipCtrl,
                            keyboardType: TextInputType.number,
                            formatters: [FilteringTextInputFormatter.digitsOnly, CepInputFormatter()]),
                      ),
                      const SizedBox(width: 12),
                      SizedBox(
                        height: 56,
                        child: ElevatedButton(
                          onPressed: _isSearchingCep ? null : _searchCep,
                          style: ElevatedButton.styleFrom(shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
                          child: _isSearchingCep
                              ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                              : const Text('Buscar'),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  _buildTextField('Rua', _shopStreetCtrl),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(flex: 2, child: _buildTextField('Número', _shopNumberCtrl, keyboardType: TextInputType.number)),
                      const SizedBox(width: 12),
                      Expanded(flex: 3, child: _buildTextField('Complemento', _shopComplementCtrl)),
                    ],
                  ),
                  const SizedBox(height: 12),
                  _buildTextField('Bairro', _shopNeighborhoodCtrl),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(flex: 3, child: _buildTextField('Cidade', _shopCityCtrl)),
                      const SizedBox(width: 12),
                      Expanded(flex: 1, child: _buildTextField('UF', _shopStateCtrl)),
                    ],
                  ),
                  const SizedBox(height: 20),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _isSavingShop ? null : _saveBarbershop,
                      style: ElevatedButton.styleFrom(
                        minimumSize: const Size(double.infinity, 50),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      child: _isSavingShop
                          ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                          : const Text('Salvar Dados da Barbearia'),
                    ),
                  ),
                  const SizedBox(height: 32),
                  const Divider(),
                  const SizedBox(height: 24),
                  _buildSectionTitle('Meu Perfil'),
                  const SizedBox(height: 20),
                  Center(
                    child: GestureDetector(
                      onTap: _pickImage,
                      child: Stack(
                        children: [
                          CircleAvatar(
                            radius: 50,
                            backgroundColor: Theme.of(context).dividerColor,
                            backgroundImage: _getProfileImage(),
                            child: _photo.isEmpty
                                ? Icon(Icons.person, size: 50, color: Theme.of(context).iconTheme.color)
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
                  const SizedBox(height: 24),
                  _buildTextField('Nome', _profileNameCtrl),
                  const SizedBox(height: 12),
                  _buildTextField('E-mail', _profileEmailCtrl, keyboardType: TextInputType.emailAddress),
                  const SizedBox(height: 12),
                  _buildTextField('WhatsApp', _profileWhatsCtrl,
                      keyboardType: TextInputType.phone,
                      formatters: [FilteringTextInputFormatter.digitsOnly, TelefoneInputFormatter()]),
                  const SizedBox(height: 20),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _isSavingProfile ? null : _saveProfile,
                      style: ElevatedButton.styleFrom(
                        minimumSize: const Size(double.infinity, 50),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                      child: _isSavingProfile
                          ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                          : const Text('Salvar Perfil'),
                    ),
                  ),
                  const SizedBox(height: 32),
                  const Divider(),
                  const SizedBox(height: 24),
                  _buildSectionTitle('Preferências'),
                  const SizedBox(height: 16),
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
                  const SizedBox(height: 100),
                ],
              ),
            ),
    );
  }

  Widget _buildSectionTitle(String title) {
    return Text(
      title,
      style: TextStyle(
        fontSize: 18,
        fontWeight: FontWeight.bold,
        color: Theme.of(context).textTheme.titleLarge?.color,
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
