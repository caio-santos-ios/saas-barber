import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-agenda',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './agenda.html',
  styleUrls: ['./agenda.css']
})
export class Agenda implements OnInit {
  selectedDate: Date = new Date();
  appointments: any[] = [];

  isModalOpen = false;
  barbers: any[] = [];
  customers: any[] = [];
  services: any[] = [];
  availableSlots: string[] = [];

  formData = {
    customerId: '',
    barberId: '',
    serviceId: '',
    date: new Date().toISOString().split('T')[0],
    hour: ''
  };

  constructor(private toastr: ToastrService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadAppointments();
    this.loadSelectOptions();
  }

  async loadAppointments() {
    try {
      const barbershopId = localStorage.getItem('barbershopId');

      const [aptsRes, usersRes] = await Promise.all([
        api.get(`/appointments?barbershopId=${barbershopId}`),
        api.get(`/users?barbershopId=${barbershopId}`)
      ]);

      const users: any[] = usersRes.data.data || [];
      const userMap: Record<string, string> = Object.fromEntries(
        users.map((u: any) => [u.id, u.name])
      );

      const dateString = this.selectedDate.toISOString().split('T')[0];
      const statusMap: any = { 0: 'Agendado', 1: 'Agendado', 2: 'Cancelado', 3: 'Concluído' };

      this.appointments = (aptsRes.data.data || [])
        .filter((apt: any) => apt.date.startsWith(dateString))
        .map((apt: any) => ({
          ...apt,
          barberName: userMap[apt.barberId] || apt.barberName || '-',
          customerName: userMap[apt.customerId] || apt.customerName || '-',
          statusLabel: statusMap[apt.status] ?? apt.status,
          statusRaw: apt.status
        }))
        .sort((a: any, b: any) => a.hour.localeCompare(b.hour));

      this.cdr.detectChanges();
    } catch (err) {
      console.error('Erro ao carregar agendamentos', err);
    }
  }

  async loadSelectOptions() {
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      
      const usersRes = await api.get(`/users?barbershopId=${barbershopId}`);
      const users = usersRes.data.data || [];
      this.barbers = users.filter((u: any) => u.role === 'Barber' || u.role === 1);
      this.customers = users.filter((u: any) => u.role === 'Customer' || u.role === 2);

      const servicesRes = await api.get(`/services_types?barbershopId=${barbershopId}`);
      this.services = servicesRes.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar opções', err);
    }
  }

  async loadAvailableSlots() {
    if (!this.formData.barberId || !this.formData.date) return;
    
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      const res = await api.get(`/appointments/availability?barberId=${this.formData.barberId}&date=${this.formData.date}&barbershopId=${barbershopId}`);
      this.availableSlots = res.data.data || [];
      this.formData.hour = '';
    } catch (err) {
      console.error('Erro ao buscar horários', err);
      this.availableSlots = [];
    }
  }

  onBarberOrDateChange() {
    this.loadAvailableSlots();
  }

  changeDate(days: number) {
    this.selectedDate.setDate(this.selectedDate.getDate() + days);
    this.selectedDate = new Date(this.selectedDate);
    this.loadAppointments();
  }

  openModal() {
    this.formData = {
      customerId: '',
      barberId: '',
      serviceId: '',
      date: this.selectedDate.toISOString().split('T')[0],
      hour: ''
    };
    this.availableSlots = [];
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  isFormValid(): boolean {
    return !!this.formData.hour && !!this.formData.customerId && !!this.formData.serviceId;
  }

  async saveAppointment() {
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    try {
      const barbershopId = localStorage.getItem('barbershopId');

      const customer = this.customers.find(c => c.id === this.formData.customerId);
      const service = this.services.find(s => s.id === this.formData.serviceId);
      
      const payload: any = {
        ...this.formData,
        serviceTypeId: this.formData.serviceId,
        date: new Date(this.formData.date).toISOString(),
        customerName: customer?.name || '',
        serviceTypeName: service?.name || '',
        value: service?.value || 0,
        barbershopId: barbershopId
      };

      if (!payload.id) {
        delete payload.id;
      }

      await api.post(`/appointments?barbershopId=${barbershopId}`, payload);
      
      this.toastr.success('Agendamento salvo com sucesso!', 'Sucesso');
      this.closeModal();
      this.loadAppointments();
    } catch (error) {
      const err = error as any;
      console.error('Erro ao salvar agendamento', err);
      const msg = err.response?.data?.message || 'Horário indisponível ou erro ao agendar.';
      this.toastr.error(msg, 'Erro');
    }
  }
}
