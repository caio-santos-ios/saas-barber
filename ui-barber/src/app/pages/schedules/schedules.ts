import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-schedules',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './schedules.html',
  styleUrls: []
})
export class Schedules implements OnInit {
  schedules: any[] = [];
  barbers: any[] = [];
  loading = true;

  isModalOpen = false;
  isDeleteModalOpen = false;
  isEditing = false;
  scheduleToDelete: any = null;
  
  daysOfWeek = [
    { value: 0, label: 'Segunda-feira' },
    { value: 1, label: 'Terça-feira' },
    { value: 2, label: 'Quarta-feira' },
    { value: 3, label: 'Quinta-feira' },
    { value: 4, label: 'Sexta-feira' },
    { value: 5, label: 'Sábado' },
    { value: 6, label: 'Domingo' }
  ];

  formData = {
    id: '',
    barberId: '',
    day: 0,
    startHour: '09:00',
    endHour: '18:00',
    intervalMinutes: 30,
    notes: ''
  };

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadInitialData();
  }

  async loadInitialData() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      const [schedRes, barbRes] = await Promise.all([
        api.get(`/schedules?barbershopId=${barbershopId}`),
        api.get(`/users?role=Barber&barbershopId=${barbershopId}`)
      ]);
      
      this.schedules = schedRes.data.data || [];
      this.barbers = barbRes.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar dados', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  getDayName(dayValue: number): string {
    const day = this.daysOfWeek.find(d => d.value === dayValue);
    return day ? day.label : 'Desconhecido';
  }

  getBarberName(barberId: string): string {
    const barber = this.barbers.find(b => b.id === barberId);
    return barber ? barber.name : 'Barbeiro Removido';
  }

  formatTimeSpanToTime(timeSpan: string): string {
    if (!timeSpan) return '00:00';
    // TimeSpan from backend comes as "09:00:00"
    return timeSpan.substring(0, 5);
  }

  openModal(sched?: any) {
    if (sched) {
      this.isEditing = true;
      this.formData = {
        id: sched.id,
        barberId: sched.barberId,
        day: sched.day,
        startHour: this.formatTimeSpanToTime(sched.startHour),
        endHour: this.formatTimeSpanToTime(sched.endHour),
        intervalMinutes: sched.intervalMinutes || 30,
        notes: sched.notes || ''
      };
    } else {
      this.isEditing = false;
      this.formData = { 
        id: '', 
        barberId: this.barbers.length > 0 ? this.barbers[0].id : '', 
        day: 0, 
        startHour: '09:00', 
        endHour: '18:00',
        intervalMinutes: 30,
        notes: ''
      };
    }
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  isFormValid(): boolean {
    return !!this.formData.barberId && !!this.formData.startHour && !!this.formData.endHour && this.formData.intervalMinutes > 0;
  }

  async saveSchedule() {
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }

    try {
      const barbershopId = localStorage.getItem('barbershopId');
      
      const payload: any = {
        barberId: this.formData.barberId,
        day: Number(this.formData.day),
        startHour: `${this.formData.startHour}:00`,
        endHour: `${this.formData.endHour}:00`,
        intervalMinutes: this.formData.intervalMinutes,
        notes: this.formData.notes || '',
        barbershopId: barbershopId
      };

      if (this.isEditing) {
        await api.put(`/schedules/${this.formData.id}?barbershopId=${barbershopId}`, payload);
      } else {
        await api.post(`/schedules?barbershopId=${barbershopId}`, payload);
      }
      
      this.toastr.success('Escala salva com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadInitialData();
    } catch (err) {
      console.error('Erro ao salvar escala', err);
      this.toastr.error('Erro ao salvar escala. Tente novamente.', 'Erro');
    } finally {
      this.cdr.detectChanges();
    }
  }

  openDeleteModal(sched: any) {
    this.scheduleToDelete = sched;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.scheduleToDelete = null;
  }

  async confirmDelete() {
    if (!this.scheduleToDelete) return;
    
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      await api.delete(`/schedules/${this.scheduleToDelete.id}?barbershopId=${barbershopId}`);
      
      this.toastr.success('Escala excluída com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadInitialData();
    } catch (err) {
      console.error('Erro ao excluir escala', err);
      this.toastr.error('Erro ao excluir escala. Tente novamente.', 'Erro');
    } finally {
      this.cdr.detectChanges();
    }
  }
}
