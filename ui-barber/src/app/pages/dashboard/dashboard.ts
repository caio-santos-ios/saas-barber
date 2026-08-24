import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  appointments: any[] = [];
  metrics: any = null;
  loading = true;
  error = '';
  today = new Date();
  
  startDate: string = '';
  endDate: string = '';

  constructor(private cdr: ChangeDetectorRef, private ngZone: NgZone) {}

  ngOnInit() {

    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 30);
    
    this.startDate = start.toISOString().split('T')[0];
    this.endDate = end.toISOString().split('T')[0];

    this.fetchData();
  }

  fetchData() {
    this.loading = true;
    this.error = '';
    this.cdr.detectChanges();

    Promise.all([
      api.get(`/appointments`),
      api.get(`/web/dashboard?startDate=${this.startDate}&endDate=${this.endDate}`),
      api.get(`/users/barbers?deleted=false`)
    ]).then(([appointmentsRes, metricsRes, usersRes]) => {
      this.ngZone.run(() => {
        const users: any[] = usersRes.data?.data || [];
        const userMap: Record<string, string> = Object.fromEntries(
          users.map((u: any) => [u.id, u.name])
        );

        const statusMap: any = { 0: 'Agendado', 1: 'Agendado', 2: 'Cancelado', 3: 'Concluído' };
        const rawApts: any[] = appointmentsRes.data?.data || [];

        const start = new Date(this.startDate + 'T00:00:00');
        const end = new Date(this.endDate + 'T23:59:59');

        this.appointments = rawApts
          .filter((apt: any) => {
            const d = new Date(apt.date);
            return d >= start && d <= end;
          })
          .map((apt: any) => ({
            ...apt,
            barberName: userMap[apt.barberId] || apt.barberName || '-',
            customerName: userMap[apt.customerId] || apt.customerName || '-',
            statusLabel: statusMap[apt.status] ?? apt.status,
            statusRaw: apt.status
          }))
          .sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime());

        this.metrics = metricsRes.data?.data || null;
        this.loading = false;

        if (this.metrics && this.metrics.totalAppointments === 0) {
           this.metrics.isEmpty = true;
        }

        this.cdr.detectChanges();
      });
    }).catch((err: any) => {
      this.ngZone.run(() => {
        console.error(err);
        this.error = 'Erro ao carregar dados do dashboard. Verifique sua conexão ou se a API está rodando.';
        this.loading = false;
        this.cdr.detectChanges();
      });
    });
  }

  onFilterChange() {
    this.fetchData();
  }
}
