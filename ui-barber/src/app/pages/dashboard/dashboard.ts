import { Component, OnInit, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit, OnDestroy {
  appointments: any[] = [];
  metrics: any = null;
  loading = true;
  error = '';
  today = new Date();
  
  startDate: string = '';
  endDate: string = '';

  private revenueChartInstance: Chart | null = null;
  private hourlyChartInstance: Chart | null = null;
  private statusChartInstance: Chart | null = null;
  private barberChartInstance: Chart | null = null;

  constructor(private cdr: ChangeDetectorRef, private ngZone: NgZone) {}

  ngOnInit() {
    const start = new Date();
    start.setDate(start.getDate() - 30);
    const end = new Date();
    end.setDate(end.getDate() + 30);
    
    this.startDate = start.toISOString().split('T')[0];
    this.endDate = end.toISOString().split('T')[0];

    this.fetchData();
  }

  ngOnDestroy() {
    this.destroyCharts();
  }

  destroyCharts() {
    if (this.revenueChartInstance) {
      this.revenueChartInstance.destroy();
      this.revenueChartInstance = null;
    }
    if (this.hourlyChartInstance) {
      this.hourlyChartInstance.destroy();
      this.hourlyChartInstance = null;
    }
    if (this.statusChartInstance) {
      this.statusChartInstance.destroy();
      this.statusChartInstance = null;
    }
    if (this.barberChartInstance) {
      this.barberChartInstance.destroy();
      this.barberChartInstance = null;
    }
  }

  fetchData() {
    this.loading = true;
    this.error = '';
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

        const statusMap: any = {
          0: 'Agendado',
          1: 'Cancelado',
          2: 'Concluído',
          3: 'Cancelado',
          'Marcado': 'Agendado',
          'Cancelado': 'Cancelado',
          'Finalizado': 'Concluído'
        };
        const rawApts: any[] = appointmentsRes.data?.data || [];
        this.appointments = rawApts.map((apt: any) => ({...apt, statusLabel: statusMap[apt.status] ?? apt.status, statusRaw: apt.status}));

        this.metrics = metricsRes.data?.data || null;
        this.error = '';
        this.loading = false;

        if (this.metrics && this.metrics.totalAppointments === 0) {
           this.metrics.isEmpty = true;
        }

        this.cdr.detectChanges();
        setTimeout(() => this.renderCharts(), 50);
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

  renderCharts() {
    this.destroyCharts();
    if (!this.metrics && this.appointments.length === 0) return;

    this.renderRevenueChart();
    this.renderHourlyChart();
    this.renderStatusChart();
    this.renderBarberChart();
  }

  renderRevenueChart() {
    const ctx = document.getElementById('revenueChart') as HTMLCanvasElement;
    const wrapper = document.getElementById('revenueChartWrapper');
    if (!ctx) return;

    let dailyData = this.metrics?.dailyRevenues;
    if (!dailyData || dailyData.length === 0) {
      const grouped: Record<string, number> = {};
      this.appointments.forEach((apt: any) => {
        const d = new Date(apt.date);
        const dayKey = `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}`;
        if (apt.statusRaw === 2 || apt.statusRaw === 3 || apt.statusLabel === 'Concluído') {
          grouped[dayKey] = (grouped[dayKey] || 0) + Number(apt.price || 0);
        } else if (!grouped[dayKey]) {
          grouped[dayKey] = 0;
        }
      });
      dailyData = Object.entries(grouped)
        .map(([date, revenue]) => ({ date, revenue }))
        .sort((a, b) => a.date.localeCompare(b.date));
    }

    if (wrapper) {
      wrapper.style.minWidth = dailyData.length > 25 ? `${dailyData.length * 30}px` : '100%';
    }

    const labels = dailyData.map((d: any) => d.date);
    const data = dailyData.map((d: any) => d.revenue);

    this.revenueChartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels.length > 0 ? labels : ['Sem dados'],
        datasets: [{
          label: 'Faturamento (R$)',
          data: data.length > 0 ? data : [0],
          borderColor: '#d4af37',
          backgroundColor: 'rgba(212, 175, 55, 0.12)',
          fill: true,
          tension: 0.35,
          pointBackgroundColor: '#d4af37',
          pointRadius: 4,
          pointHoverRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (context) => ` R$ ${(context.raw as number).toFixed(2).replace('.', ',')}`
            }
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: {
              autoSkip: true,
              maxTicksLimit: 12,
              maxRotation: 0
            }
          },
          y: {
            beginAtZero: true,
            ticks: {
              callback: (value) => `R$ ${value}`
            }
          }
        }
      }
    });
  }

  renderHourlyChart() {
    const ctx = document.getElementById('hourlyChart') as HTMLCanvasElement;
    const wrapper = document.getElementById('hourlyChartWrapper');
    if (!ctx) return;

    let hourlyData = this.metrics?.hourlyDistribution;
    if (!hourlyData || hourlyData.length === 0) {
      const grouped: Record<string, number> = {};
      this.appointments.forEach((apt: any) => {
        const h = (apt.hour || '').substring(0, 5);
        if (h) {
          grouped[h] = (grouped[h] || 0) + 1;
        }
      });
      hourlyData = Object.entries(grouped)
        .map(([hour, count]) => ({ hour, count }))
        .sort((a, b) => a.hour.localeCompare(b.hour));
    }

    if (wrapper) {
      wrapper.style.minWidth = hourlyData.length > 15 ? `${hourlyData.length * 38}px` : '100%';
    }

    const labels = hourlyData.map((h: any) => h.hour);
    const data = hourlyData.map((h: any) => h.count);

    this.hourlyChartInstance = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels.length > 0 ? labels : ['Sem dados'],
        datasets: [{
          label: 'Agendamentos',
          data: data.length > 0 ? data : [0],
          backgroundColor: '#3b82f6',
          borderRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: {
              autoSkip: true,
              maxTicksLimit: 12,
              maxRotation: 0
            }
          },
          y: {
            beginAtZero: true,
            ticks: { stepSize: 1 }
          }
        }
      }
    });
  }

  renderStatusChart() {
    const ctx = document.getElementById('statusChart') as HTMLCanvasElement;
    if (!ctx) return;

    const completed = this.metrics?.completedAppointments ?? 0;
    const scheduled = (this.metrics?.scheduledAppointments ?? this.metrics?.confirmedAppointments) ?? 0;
    const canceled = this.metrics?.canceledAppointments ?? 0;

    const hasData = completed > 0 || scheduled > 0 || canceled > 0;

    this.statusChartInstance = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Concluídos', 'Agendados', 'Cancelados'],
        datasets: [{
          data: hasData ? [completed, scheduled, canceled] : [0, 0, 0],
          backgroundColor: ['#10b981', '#3b82f6', '#ef4444'],
          borderWidth: 2,
          hoverOffset: 4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { boxWidth: 12, padding: 12 }
          }
        },
        cutout: '70%'
      }
    });
  }

  renderBarberChart() {
    const ctx = document.getElementById('barberChart') as HTMLCanvasElement;
    const wrapper = document.getElementById('barberChartWrapper');
    if (!ctx) return;

    let barberData = this.metrics?.barberRevenues;
    if (!barberData || barberData.length === 0) {
      const grouped: Record<string, number> = {};
      this.appointments.forEach((apt: any) => {
        const name = apt.barberName || 'Profissional';
        if (apt.statusRaw === 2 || apt.statusRaw === 3 || apt.statusLabel === 'Concluído') {
          grouped[name] = (grouped[name] || 0) + Number(apt.price || 0);
        } else if (!grouped[name]) {
          grouped[name] = 0;
        }
      });
      barberData = Object.entries(grouped)
        .map(([name, revenue]) => ({ name, revenue }))
        .sort((a, b) => b.revenue - a.revenue);
    }

    if (wrapper) {
      wrapper.style.height = barberData.length > 5 ? `${barberData.length * 36}px` : '200px';
    }

    const labels = barberData.map((b: any) => b.name);
    const data = barberData.map((b: any) => b.revenue);

    this.barberChartInstance = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels.length > 0 ? labels : ['Sem dados'],
        datasets: [{
          label: 'Faturamento (R$)',
          data: data.length > 0 ? data : [0],
          backgroundColor: '#8b5cf6',
          borderRadius: 6
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (context) => ` R$ ${(context.raw as number).toFixed(2).replace('.', ',')}`
            }
          }
        },
        scales: {
          x: {
            beginAtZero: true,
            ticks: {
              callback: (value) => `R$ ${value}`
            }
          },
          y: { grid: { display: false } }
        }
      }
    });
  }

  onFilterChange() {
    this.fetchData();
  }
}
