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
  styleUrls: ['./schedules.css']
})
export class Schedules implements OnInit {
  schedules: any[] = [];
  barbers: any[] = [];
  loading = true;

  selectedBarberFilter: string = '';

  isModalOpen = false;
  isBatchModalOpen = false;
  isDeleteModalOpen = false;
  isEditing = false;
  isSaving = false;
  isSavingBatch = false;
  isDeleting = false;
  scheduleToDelete: any = null;

  daysOfWeek = [
    { value: 0, label: 'Segunda', short: 'Seg' },
    { value: 1, label: 'Terça', short: 'Ter' },
    { value: 2, label: 'Quarta', short: 'Qua' },
    { value: 3, label: 'Quinta', short: 'Qui' },
    { value: 4, label: 'Sexta', short: 'Sex' },
    { value: 5, label: 'Sábado', short: 'Sáb' },
    { value: 6, label: 'Domingo', short: 'Dom' }
  ];

  formData = {
    id: '',
    barberId: '',
    day: 0,
    startHour: '09:00',
    endHour: '18:00',
    breakStart: '',
    breakEnd: '',
    intervalMinutes: 30,
    notes: ''
  };

  batchFormData = {
    barberId: '',
    startHour: '09:00',
    endHour: '18:00',
    breakStart: '',
    breakEnd: '',
    intervalMinutes: 30,
    selectedDays: [0, 1, 2, 3, 4, 5],
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
      const barbershopId = localStorage.getItem('barbershopId') || '';
      const [schedRes, barbRes] = await Promise.all([
        api.get(`/schedules?deleted=false&barbershopId=${barbershopId}`),
        api.get(`/users/barbers?deleted=false&barbershopId=${barbershopId}`)
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

  get filteredBarbers(): any[] {
    if (!this.selectedBarberFilter) {
      return this.barbers;
    }
    return this.barbers.filter(b => b.id === this.selectedBarberFilter);
  }

  getScheduleForBarberAndDay(barberId: string, day: number): any {
    return this.schedules.find(s => s.barberId === barberId && s.day === day) || null;
  }

  getActiveDaysCount(barberId: string): number {
    return this.schedules.filter(s => s.barberId === barberId).length;
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
    if (!timeSpan) return '';
    return timeSpan.substring(0, 5);
  }

  async openModal(sched?: any) {
    const schedId = typeof sched === 'string' ? sched : sched?.id;
    if (schedId) {
      this.isEditing = true;
      await this.getById(schedId);
    } else {
      this.isModalOpen = true;
      this.isEditing = false;
      this.formData = { 
        id: '', 
        barberId: this.barbers.length > 0 ? this.barbers[0].id : '', 
        day: 0, 
        startHour: '09:00', 
        endHour: '18:00',
        breakStart: '',
        breakEnd: '',
        intervalMinutes: 30,
        notes: ''
      };
    }
  }

  openModalForSlot(barberId: string, day: number) {
    const existing = this.getScheduleForBarberAndDay(barberId, day);
    if (existing) {
      this.openModal(existing);
    } else {
      this.isModalOpen = true;
      this.isEditing = false;
      this.formData = {
        id: '',
        barberId,
        day,
        startHour: '09:00',
        endHour: '18:00',
        breakStart: '',
        breakEnd: '',
        intervalMinutes: 30,
        notes: ''
      };
    }
  }

  async getById(id: string) {
    try {
      this.isModalOpen = true;
      const response = await api.get(`/schedules/${id}`);
      const sched = response.data.data;
      this.formData = {
        id: sched.id,
        barberId: sched.barberId,
        day: sched.day,
        startHour: this.formatTimeSpanToTime(sched.startHour) || '09:00',
        endHour: this.formatTimeSpanToTime(sched.endHour) || '18:00',
        breakStart: this.formatTimeSpanToTime(sched.breakStart) || '',
        breakEnd: this.formatTimeSpanToTime(sched.breakEnd) || '',
        intervalMinutes: sched.intervalMinutes || 30,
        notes: sched.notes || ''
      };
      this.cdr.detectChanges();
    } catch (error) {
      this.toastr.error('Erro ao buscar dados da escala', 'Erro');
    }
  }

  closeModal() {
    this.isModalOpen = false;
  }

  isFormValid(): boolean {
    return !!this.formData.barberId && !!this.formData.startHour && !!this.formData.endHour && this.formData.intervalMinutes > 0;
  }

  async saveSchedule() {
    if (this.isSaving) return;
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }

    this.isSaving = true;
    this.cdr.detectChanges();
    try {
      const payload: any = {
        id: this.formData.id,
        barberId: this.formData.barberId,
        day: Number(this.formData.day),
        startHour: `${this.formData.startHour}:00`,
        endHour: `${this.formData.endHour}:00`,
        breakStart: this.formData.breakStart ? `${this.formData.breakStart}:00` : null,
        breakEnd: this.formData.breakEnd ? `${this.formData.breakEnd}:00` : null,
        intervalMinutes: this.formData.intervalMinutes,
        notes: this.formData.notes || ''
      };

      if (!payload.id) {
        delete payload.id;
      }

      if (this.isEditing) {
        await api.put(`/schedules`, payload);
      } else {
        await api.post(`/schedules`, payload);
      }
      
      this.toastr.success('Escala salva com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadInitialData();
    } catch (err) {
      console.error('Erro ao salvar escala', err);
      this.toastr.error('Erro ao salvar escala. Tente novamente.', 'Erro');
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }

  openBatchModal(barberId?: string) {
    this.batchFormData = {
      barberId: barberId || (this.barbers.length > 0 ? this.barbers[0].id : ''),
      startHour: '09:00',
      endHour: '18:00',
      breakStart: '',
      breakEnd: '',
      intervalMinutes: 30,
      selectedDays: [0, 1, 2, 3, 4, 5],
      notes: ''
    };
    this.isBatchModalOpen = true;
  }

  closeBatchModal() {
    this.isBatchModalOpen = false;
  }

  toggleBatchDay(dayValue: number) {
    const idx = this.batchFormData.selectedDays.indexOf(dayValue);
    if (idx > -1) {
      this.batchFormData.selectedDays.splice(idx, 1);
    } else {
      this.batchFormData.selectedDays.push(dayValue);
    }
  }

  isBatchDaySelected(dayValue: number): boolean {
    return this.batchFormData.selectedDays.includes(dayValue);
  }

  async saveBatchSchedule() {
    if (this.isSavingBatch) return;
    if (!this.batchFormData.barberId) {
      this.toastr.warning('Selecione um profissional.', 'Atenção');
      return;
    }

    if (this.batchFormData.selectedDays.length === 0) {
      this.toastr.warning('Selecione pelo menos um dia da semana.', 'Atenção');
      return;
    }

    this.isSavingBatch = true;
    this.cdr.detectChanges();
    try {
      for (const day of this.batchFormData.selectedDays) {
        const existing = this.getScheduleForBarberAndDay(this.batchFormData.barberId, day);
        const payload: any = {
          barberId: this.batchFormData.barberId,
          day,
          startHour: `${this.batchFormData.startHour}:00`,
          endHour: `${this.batchFormData.endHour}:00`,
          breakStart: this.batchFormData.breakStart ? `${this.batchFormData.breakStart}:00` : null,
          breakEnd: this.batchFormData.breakEnd ? `${this.batchFormData.breakEnd}:00` : null,
          intervalMinutes: this.batchFormData.intervalMinutes,
          notes: this.batchFormData.notes || ''
        };

        if (existing) {
          payload.id = existing.id;
          await api.put(`/schedules`, payload);
        } else {
          await api.post(`/schedules`, payload);
        }
      }

      this.toastr.success('Escala semanal configurada com sucesso!', 'Sucesso');
      this.closeBatchModal();
      await this.loadInitialData();
    } catch (err) {
      console.error('Erro ao configurar escala semanal', err);
      this.toastr.error('Erro ao aplicar escalas.', 'Erro');
    } finally {
      this.isSavingBatch = false;
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
    if (this.isDeleting || !this.scheduleToDelete) return;
    this.isDeleting = true;
    this.cdr.detectChanges();
    try {
      await api.delete(`/schedules/${this.scheduleToDelete.id}`);
      this.toastr.success('Horário removido com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadInitialData();
    } catch (err) {
      console.error('Erro ao deletar horário', err);
      this.toastr.error('Erro ao remover horário.', 'Erro');
    } finally {
      this.isDeleting = false;
      this.cdr.detectChanges();
    }
  }
}
