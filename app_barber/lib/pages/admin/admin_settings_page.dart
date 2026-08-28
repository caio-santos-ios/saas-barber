import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:hive/hive.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/barbershop.dart';
import 'package:app_barber/repositories/barbershop_repository.dart';
import 'package:brasil_fields/brasil_fields.dart';
import 'package:http/http.dart' as http;

class AdminSettingsPage extends StatefulWidget {
  const AdminSettingsPage({super.key});

  @override
  State<AdminSettingsPage> createState() => _AdminSettingsPageState();
}

class _AdminSettingsPageState extends State<AdminSettingsPage> {
  final ApiClient _apiClient = ApiClient();
  late final BarbershopRepository _barbershopRepo;

  bool _isLoading = true;
  bool _isSavingShop = false;
  bool _isSearchingCep = false;

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

  String _barbershopId = '';

  @override
  void initState() {
    super.initState();
    _barbershopRepo = BarbershopRepository(_apiClient);
    _loadBarbershop();
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
    super.dispose();
  }

  Future<void> _loadBarbershop() async {
    setState(() => _isLoading = true);
    final authBox = Hive.box('auth');
    _barbershopId = authBox.get('barbershopId', defaultValue: '');

    try {
      final shop = await _barbershopRepo.getBarbershop(_barbershopId);

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
    } catch (_) {
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
                  const SizedBox(height: 24),
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
