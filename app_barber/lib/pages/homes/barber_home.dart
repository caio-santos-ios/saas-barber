import 'package:flutter/material.dart';
import 'package:app_barber/pages/barber/barber_dashboard_page.dart';
import 'package:app_barber/pages/barber/barber_services_page.dart';
import 'package:app_barber/pages/barber/barber_finance_page.dart';
import 'package:app_barber/pages/barber/barber_profile_page.dart';

class BarberHomePage extends StatefulWidget {
  const BarberHomePage({super.key});

  @override
  State<BarberHomePage> createState() => _BarberHomePageState();
}

class _BarberHomePageState extends State<BarberHomePage> {
  int _currentIndex = 0;

  final List<Widget> _pages = const [
    BarberDashboardPage(),
    BarberServicesPage(),
    BarberFinancePage(),
    BarberProfilePage(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _pages[_currentIndex],
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex,
        type: BottomNavigationBarType.fixed,
        onTap: (index) {
          setState(() {
            _currentIndex = index;
          });
        },
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.view_agenda),
            label: 'Agenda',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.cut),
            label: 'Serviços',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.attach_money),
            label: 'Financeiro',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person),
            label: 'Perfil',
          ),
        ],
      ),
    );
  }
}
