import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';

import { GlobalService } from '../../services/global.service';

@Component({
  selector: 'app-agenda',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './agenda.html',
  styleUrls: ['./agenda.css']
})
export class Agenda extends GlobalService implements OnInit {
  selectedDate: Date = new Date();
  appointments: any[] = [];
  activeStatusDropdownId: string | null = null;

  isModalOpen = false;
  isSaving = false;
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

  constructor(private cdr: ChangeDetectorRef) {
    super();
  }

  ngOnInit() {
    this.loadAppointments();
    this.loadSelectOptions();
  }

  async loadAppointments() {
    this.spinnerShow();
    try {
      const barbershopId = localStorage.getItem('barbershopId') || '';
      const [aptsRes, usersRes] = await Promise.all([
        api.get(`/appointments?deleted=false&barbershopId=${barbershopId}`),
        api.get(`/users?deleted=false&barbershopId=${barbershopId}`)
      ]);

      const users: any[] = usersRes.data.data || [];
      const userMap: Record<string, string> = Object.fromEntries(
        users.map((u: any) => [u.id, u.name])
      );

      const dateString = this.selectedDate.toISOString().split('T')[0];
      const getStatusInfo = (status: any) => {
        const s = typeof status === 'string' ? status.trim().toLowerCase() : status;
        if (s === 0 || s === '0' || s === 'marcado' || s === 'agendado') {
          return { label: 'AGENDADO', code: 'scheduled' };
        }
        if (s === 2 || s === '2' || s === 'finalizado' || s === 'concluído' || s === 'concluido') {
          return { label: 'CONCLUÍDO', code: 'completed' };
        }
        if (s === 1 || s === '1' || s === 'cancelado') {
          return { label: 'CANCELADO', code: 'cancelled' };
        }
        if (s === 3 || s === '3' || s === 'naorealizado' || s === 'não realizado' || s === 'nao realizado') {
          return { label: 'NÃO REALIZADO', code: 'not-done' };
        }
        return { label: 'AGENDADO', code: 'scheduled' };
      };

      this.appointments = (aptsRes.data.data || [])
        .filter((apt: any) => apt.date.startsWith(dateString))
        .map((apt: any) => {
          const st = getStatusInfo(apt.status);
          return {
            ...apt,
            barberName: userMap[apt.barberId] || apt.barberName || '-',
            customerName: userMap[apt.customerId] || apt.customerName || '-',
            statusLabel: st.label,
            statusCode: st.code,
            statusRaw: apt.status
          };
        })
        .sort((a: any, b: any) => a.hour.localeCompare(b.hour));

      this.cdr.detectChanges();
    } catch (err) {
      this.errorNotification(err);
      console.error('Erro ao carregar agendamentos', err);
    } finally {
      this.spinnerHide();
    }
  }

  async loadSelectOptions() {
    try {
      const barbershopId = localStorage.getItem('barbershopId') || '';
      const usersRes = await api.get(`/users?deleted=false&barbershopId=${barbershopId}`);
      const users = usersRes.data.data || [];
      this.barbers = users.filter((u: any) => u.role === 'Barber' || u.role === 1);
      this.customers = users.filter((u: any) => u.role === 'Customer' || u.role === 2);

      const servicesRes = await api.get(`/services_types?deleted=false&barbershopId=${barbershopId}`);
      this.services = servicesRes.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar opções', err);
    }
  }

  async loadAvailableSlots() {
    if (!this.formData.barberId || !this.formData.date) return;
    
    try {
      const params = new URLSearchParams({
        barberId: this.formData.barberId,
        date: this.formData.date
      });
      if (this.formData.serviceId) {
        params.append('serviceId', this.formData.serviceId);
      }
      if (this.formData.customerId) {
        params.append('customerId', this.formData.customerId);
      }
      const res = await api.get(`/appointments/availability?${params.toString()}`);
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

  async openModal(apt?: any) {
    const aptId = typeof apt === 'string' ? apt : apt?.id;
    if (aptId) {
      await this.getById(aptId);
    } else {
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
  }

  async getById(id: string) {
    try {
      this.isModalOpen = true;
      const response = await api.get(`/appointments/${id}`);
      const apt = response.data.data;
      this.formData = {
        customerId: apt.customerId || '',
        barberId: apt.barberId || '',
        serviceId: apt.serviceTypeId || apt.serviceId || '',
        date: (apt.date || '').split('T')[0],
        hour: apt.hour || ''
      };
      if (this.formData.barberId && this.formData.date) {
        await this.loadAvailableSlots();
      }
      this.cdr.detectChanges();
    } catch (error) {
      this.toastr.error('Erro ao buscar dados do agendamento', 'Erro');
    }
  }

  closeModal() {
    this.isModalOpen = false;
  }

  isFormValid(): boolean {
    return !!this.formData.hour && !!this.formData.customerId && !!this.formData.serviceId;
  }

  async saveAppointment() {
    if (this.isSaving) return;
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    this.isSaving = true;
    this.cdr.detectChanges();
    try {
      const customer = this.customers.find(c => c.id === this.formData.customerId);
      const service = this.services.find(s => s.id === this.formData.serviceId);
      
      const payload: any = {
        ...this.formData,
        serviceTypeId: this.formData.serviceId,
        date: new Date(this.formData.date).toISOString(),
        customerName: customer?.name || '',
        serviceTypeName: service?.name || '',
        value: service?.value || 0
      };

      if (!payload.id) {
        delete payload.id;
      }

      await api.post(`/appointments`, payload);
      
      this.toastr.success('Agendamento salvo com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadAppointments();
    } catch (error) {
      const err = error as any;
      console.error('Erro ao salvar agendamento', err);
      const msg = err.response?.data?.message || 'Horário indisponível ou erro ao agendar.';
      this.toastr.error(msg, 'Erro');
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }

  getStatusNumeric(status: any): number {
    const s = typeof status === 'string' ? status.trim().toLowerCase() : status;
    if (s === 0 || s === '0' || s === 'marcado' || s === 'agendado') return 0;
    if (s === 1 || s === '1' || s === 'cancelado') return 1;
    if (s === 2 || s === '2' || s === 'finalizado' || s === 'concluído' || s === 'concluido') return 2;
    if (s === 3 || s === '3' || s === 'naorealizado' || s === 'não realizado' || s === 'nao realizado') return 3;
    return 0;
  }

  async updateStatus(apt: any, newStatus: number) {
    try {
      apt.status = newStatus;
      await api.put('/appointments/status', {
        id: apt.id,
        status: newStatus
      });
      this.toastr.success('Status atualizado com sucesso!', 'Sucesso');
      await this.loadAppointments();
    } catch (error) {
      console.error('Erro ao atualizar status', error);
      this.toastr.error('Erro ao atualizar status do agendamento.', 'Erro');
      await this.loadAppointments();
    }
  }

  toggleStatusDropdown(event: Event, aptId: string) {
    event.stopPropagation();
    this.activeStatusDropdownId = this.activeStatusDropdownId === aptId ? null : aptId;
  }

  closeStatusDropdown() {
    this.activeStatusDropdownId = null;
  }

  async selectStatus(event: Event, apt: any, newStatus: number) {
    event.stopPropagation();
    this.activeStatusDropdownId = null;
    if (this.getStatusNumeric(apt.status) === newStatus) return;
    await this.updateStatus(apt, newStatus);
  }

  @HostListener('document:click')
  onDocumentClick() {
    this.activeStatusDropdownId = null;
  }
}
