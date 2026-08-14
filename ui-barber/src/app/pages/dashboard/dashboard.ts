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

    const barbershopId = localStorage.getItem('barbershopId') || '';
    Promise.all([
      api.get(`/appointments?barbershopId=${barbershopId}`),
      api.get(`/web/dashboard?barbershopId=${barbershopId}&startDate=${this.startDate}T00:00:00Z&endDate=${this.endDate}T23:59:59Z`)
    ]).then(([appointmentsRes, metricsRes]) => {
      this.ngZone.run(() => {
        const statusMap: any = { 0: 'Marcado', 1: 'Concluído', 2: 'Cancelado' };
        this.appointments = (appointmentsRes.data?.data || []).map((apt: any) => ({
          ...apt,
          status: statusMap[apt.status] || apt.status
        }));
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
