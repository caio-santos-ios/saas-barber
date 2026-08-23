import 'package:flutter/material.dart';
import 'package:app_barber/models/notification.dart';
import 'package:app_barber/repositories/notification_repository.dart';
import 'package:app_barber/api/api_client.dart';
import 'package:intl/intl.dart';

class BarberNotificationsPage extends StatefulWidget {
  const BarberNotificationsPage({super.key});

  @override
  State<BarberNotificationsPage> createState() => _BarberNotificationsPageState();
}

class _BarberNotificationsPageState extends State<BarberNotificationsPage> {
  late final NotificationRepository _repo;
  List<AppNotification> _notifications = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _repo = NotificationRepository(ApiClient());
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);
    try {
      final list = await _repo.getNotifications();
      list.sort((a, b) => b.createdAt.compareTo(a.createdAt));
      if (mounted) {
        setState(() {
          _notifications = list;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Notificações'),
        centerTitle: true,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _loadData,
              child: _notifications.isEmpty
                  ? SingleChildScrollView(
                      physics: const AlwaysScrollableScrollPhysics(),
                      child: Container(
                        height: MediaQuery.of(context).size.height * 0.7,
                        alignment: Alignment.center,
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(
                              Icons.notifications_off_outlined,
                              size: 80,
                              color: Theme.of(context).dividerColor,
                            ),
                            const SizedBox(height: 16),
                            const Text(
                              'Nenhuma notificação',
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            const SizedBox(height: 8),
                            const Text(
                              'Você não tem novas mensagens ou\nlembretes no momento.',
                              textAlign: TextAlign.center,
                              style: TextStyle(
                                color: Colors.grey,
                              ),
                            ),
                          ],
                        ),
                      ),
                    )
                  : ListView.separated(
                      padding: const EdgeInsets.all(16),
                      itemCount: _notifications.length,
                      separatorBuilder: (context, index) => const Divider(),
                      itemBuilder: (context, index) {
                        final notif = _notifications[index];
                        return ListTile(
                          contentPadding: EdgeInsets.zero,
                          leading: CircleAvatar(
                            backgroundColor: notif.read ? Colors.grey[200] : Theme.of(context).primaryColor.withOpacity(0.1),
                            child: Icon(
                              Icons.notifications,
                              color: notif.read ? Colors.grey : Theme.of(context).primaryColor,
                            ),
                          ),
                          title: Text(
                            notif.title,
                            style: TextStyle(
                              fontWeight: notif.read ? FontWeight.normal : FontWeight.bold,
                            ),
                          ),
                          subtitle: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const SizedBox(height: 4),
                              Text(notif.message),
                              const SizedBox(height: 4),
                              Text(
                                DateFormat('dd/MM/yyyy').format(notif.createdAt),
                                style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                              ),
                            ],
                          ),
                        );
                      },
                    ),
            ),
    );
  }
}
