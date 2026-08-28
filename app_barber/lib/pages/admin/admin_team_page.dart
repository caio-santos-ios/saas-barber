import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:app_barber/models/user.dart';
import 'package:app_barber/repositories/barbershop_repository.dart';
import 'package:brasil_fields/brasil_fields.dart';

class AdminTeamPage extends StatefulWidget {
  const AdminTeamPage({super.key});

  @override
  State<AdminTeamPage> createState() => _AdminTeamPageState();
}

class _AdminTeamPageState extends State<AdminTeamPage> {
  late final BarbershopRepository _repo;
  List<User> _barbers = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _repo = BarbershopRepository(ApiClient());
    _loadBarbers();
  }

  Future<void> _loadBarbers() async {
    setState(() => _isLoading = true);
    final list = await _repo.getBarbershopTeam();
    if (mounted) setState(() { _barbers = list; _isLoading = false; });
  }

  void _openForm({User? barber}) {
    final nameCtrl = TextEditingController(text: barber?.name ?? '');
    final emailCtrl = TextEditingController(text: barber?.email ?? '');
    final whatsCtrl = TextEditingController(text: barber?.whatsapp ?? '');
    final docCtrl = TextEditingController(text: barber?.document ?? '');
    final passCtrl = TextEditingController();
    bool isSaving = false;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setSheet) => Padding(
          padding: EdgeInsets.only(
            left: 24, right: 24, top: 24,
            bottom: MediaQuery.of(ctx).viewInsets.bottom + 24,
          ),
          child: SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      barber == null ? 'Novo Profissional' : 'Editar Profissional',
                      style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                    ),
                    IconButton(icon: const Icon(Icons.close), onPressed: () => Navigator.pop(ctx)),
                  ],
                ),
                const SizedBox(height: 16),
                _sheetField('Nome', nameCtrl),
                const SizedBox(height: 12),
                _sheetField('E-mail', emailCtrl, keyboardType: TextInputType.emailAddress),
                const SizedBox(height: 12),
                _sheetField('WhatsApp', whatsCtrl,
                    keyboardType: TextInputType.phone,
                    formatters: [FilteringTextInputFormatter.digitsOnly, TelefoneInputFormatter()]),
                const SizedBox(height: 12),
                _sheetField('CPF', docCtrl,
                    keyboardType: TextInputType.number,
                    formatters: [FilteringTextInputFormatter.digitsOnly, CpfInputFormatter()]),
                if (barber == null) ...[
                  const SizedBox(height: 12),
                  _sheetField('Senha', passCtrl, obscure: true),
                ],
                const SizedBox(height: 24),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: isSaving
                        ? null
                        : () async {
                            setSheet(() => isSaving = true);
                            bool ok;
                            if (barber == null) {
                              ok = await _repo.createBarber({
                                'name': nameCtrl.text,
                                'email': emailCtrl.text,
                                'whatsApp': whatsCtrl.text.replaceAll(RegExp(r'\D'), ''),
                                'document': docCtrl.text.replaceAll(RegExp(r'\D'), ''),
                                'password': passCtrl.text,
                              });
                            } else {
                              ok = await _repo.updateBarber({
                                'id': barber.id,
                                'name': nameCtrl.text,
                                'email': emailCtrl.text,
                                'whatsApp': whatsCtrl.text.replaceAll(RegExp(r'\D'), ''),
                                'document': docCtrl.text.replaceAll(RegExp(r'\D'), ''),
                              });
                            }
                            setSheet(() => isSaving = false);
                            if (mounted) {
                              Navigator.pop(ctx);
                              ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                                content: Text(ok ? 'Profissional salvo com sucesso!' : 'Erro ao salvar profissional.'),
                                backgroundColor: ok ? Colors.green : Colors.red,
                              ));
                              _loadBarbers();
                            }
                          },
                    style: ElevatedButton.styleFrom(
                      minimumSize: const Size(double.infinity, 50),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    ),
                    child: isSaving
                        ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                        : const Text('Salvar'),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  void _confirmDelete(User barber) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Excluir Profissional'),
        content: Text('Deseja excluir ${barber.name}? Esta ação não pode ser desfeita.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Cancelar')),
          ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              final ok = await _repo.deleteBarber(barber.id);
              if (mounted) {
                ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                  content: Text(ok ? 'Profissional excluído com sucesso!' : 'Erro ao excluir profissional.'),
                  backgroundColor: ok ? Colors.green : Colors.red,
                ));
                _loadBarbers();
              }
            },
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red, foregroundColor: Colors.white),
            child: const Text('Excluir'),
          ),
        ],
      ),
    );
  }

  void _changePassword(User barber) {
    final passCtrl = TextEditingController();
    bool isSaving = false;

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDiag) => AlertDialog(
          title: Text('Senha de ${barber.name.split(' ').first}'),
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
                          const SnackBar(content: Text('A senha deve ter pelo menos 6 caracteres.'), backgroundColor: Colors.orange),
                        );
                        return;
                      }
                      setDiag(() => isSaving = true);
                      final ok = await _repo.changeBarberPassword(barber.id, passCtrl.text);
                      setDiag(() => isSaving = false);
                      if (mounted) {
                        Navigator.pop(ctx);
                        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                          content: Text(ok ? 'Senha alterada com sucesso!' : 'Erro ao alterar a senha.'),
                          backgroundColor: ok ? Colors.green : Colors.red,
                        ));
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

  Widget _sheetField(
    String label,
    TextEditingController ctrl, {
    TextInputType? keyboardType,
    List<TextInputFormatter>? formatters,
    bool obscure = false,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: const TextStyle(fontSize: 12, color: Colors.grey, fontWeight: FontWeight.bold)),
        const SizedBox(height: 6),
        TextFormField(
          controller: ctrl,
          keyboardType: keyboardType,
          inputFormatters: formatters,
          obscureText: obscure,
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
            contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
          ),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).scaffoldBackgroundColor,
      appBar: AppBar(
        title: Text(
          'Equipe',
          style: TextStyle(color: Theme.of(context).textTheme.titleLarge?.color),
        ),
        centerTitle: true,
        backgroundColor: Colors.transparent,
        elevation: 0,
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _openForm(),
        icon: const Icon(Icons.add),
        label: const Text('Novo Profissional'),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadBarbers,
              child: _barbers.isEmpty
                  ? _buildEmptyState()
                  : ListView.builder(
                      padding: const EdgeInsets.fromLTRB(20, 12, 20, 100),
                      itemCount: _barbers.length,
                      itemBuilder: (context, index) => _buildBarberCard(_barbers[index]),
                    ),
            ),
    );
  }

  Widget _buildBarberCard(User barber) {
    final initials = barber.name.trim().split(' ')
        .where((p) => p.isNotEmpty)
        .take(2)
        .map((p) => p[0].toUpperCase())
        .join();

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Theme.of(context).cardColor,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Theme.of(context).dividerColor),
      ),
      child: Row(
        children: [
          CircleAvatar(
            radius: 26,
            backgroundColor: Theme.of(context).colorScheme.primary.withOpacity(0.15),
            backgroundImage: (barber.photo.isNotEmpty && barber.photo.startsWith('http'))
                ? NetworkImage(barber.photo)
                : null,
            child: (barber.photo.isEmpty || !barber.photo.startsWith('http'))
                ? Text(initials, style: TextStyle(fontWeight: FontWeight.bold, color: Theme.of(context).colorScheme.primary))
                : null,
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(barber.name, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                const SizedBox(height: 2),
                Text(barber.email, style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                if (barber.whatsapp.isNotEmpty) ...[
                  const SizedBox(height: 2),
                  Text(barber.whatsapp, style: TextStyle(fontSize: 12, color: Colors.grey[500])),
                ],
              ],
            ),
          ),
          PopupMenuButton<String>(
            icon: const Icon(Icons.more_vert),
            onSelected: (value) {
              if (value == 'edit') _openForm(barber: barber);
              if (value == 'password') _changePassword(barber);
              if (value == 'delete') _confirmDelete(barber);
            },
            itemBuilder: (_) => const [
              PopupMenuItem(value: 'edit', child: Row(children: [Icon(Icons.edit, size: 18), SizedBox(width: 8), Text('Editar')])),
              PopupMenuItem(value: 'password', child: Row(children: [Icon(Icons.lock_outline, size: 18), SizedBox(width: 8), Text('Alterar Senha')])),
              PopupMenuItem(value: 'delete', child: Row(children: [Icon(Icons.delete_outline, size: 18, color: Colors.red), SizedBox(width: 8), Text('Excluir', style: TextStyle(color: Colors.red))])),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.people_outline, size: 72, color: Theme.of(context).dividerColor),
          const SizedBox(height: 16),
          const Text('Nenhum profissional cadastrado', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
          const SizedBox(height: 8),
          const Text('Toque em "+ Novo Profissional" para adicionar.', style: TextStyle(color: Colors.grey)),
        ],
      ),
    );
  }
}
