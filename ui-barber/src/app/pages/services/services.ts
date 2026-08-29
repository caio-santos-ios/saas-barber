import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { ChangeDetectorRef } from '@angular/core';
import { NgxMaskDirective } from 'ngx-mask';
import { Auth } from '../../services/auth';
import { GlobalService } from '../../services/global.service';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective],
  templateUrl: './services.html',
  styleUrls: ['./services.css']
})
export class Services implements OnInit {
  services: any[] = [];
  loading = true;

  isModalOpen = false;
  isDeleteModalOpen = false;
  isEditing = false;
  isSaving = false;
  isDeleting = false;
  serviceToDelete: any = null;
  
  formData = {
    id: '',
    name: '',
    description: '',
    durationMinutes: 30,
    price: 0,
    category: ''
  };

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef,
    private globalService: GlobalService
  ) {}

  ngOnInit() {
    this.loadServices();
  }

  async loadServices() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId') || '';
      const response = await api.get(`/services_types?deleted=false&barbershopId=${barbershopId}`);
      this.services = response.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar serviços', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  async openModal(serviceId: string) {
    
    if(serviceId) {
      this.isEditing = true;
      await this.getById(serviceId);
    } else {
      this.isModalOpen = true;
      this.isEditing = false;
      this.formData = { id: '', name: '', description: '', durationMinutes: 30, price: 0, category: 'Cabelo' };
    }
  }

  async getById(serviceId: string) {
    try {
      this.isModalOpen = true;
      const response = await api.get(`/services_types/${serviceId}`);
      this.formData = { ...response.data.data };
    } catch (error) {
      this.globalService.errorNotification(error);
    }
  }

  closeModal() {
    this.isModalOpen = false;
  }

  getParsedValue(): number {
    const stringValue = (this.formData.price || 0).toString();
    const cleanValue = stringValue.replace(/[R$\s\.]/g, '').replace(',', '.');
    return parseFloat(cleanValue) || 0;
  }

  isFormValid(): boolean {
    const name = this.formData.name || '';
    const duration = this.formData.durationMinutes || 0;
    const value = this.getParsedValue();
    
    const nameValid = name.trim().length >= 2;
    const durValid = duration > 0;
    const valValid = value > 0;
    return nameValid && durValid && valValid;
  }

  onDurationInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const raw = input.value.replace(/\D/g, '');
    const num = parseInt(raw, 10);
    this.formData.durationMinutes = isNaN(num) || num <= 0 ? 0 : num;
    input.value = this.formData.durationMinutes > 0 ? this.formData.durationMinutes.toString() : '';
  }

  async saveService() {
    if (this.isSaving) return;
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    this.isSaving = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      
      const numericValue = this.getParsedValue();
      const durationMinutes = Number(this.formData.durationMinutes) || 0;

      const payload: any = { 
        name: this.formData.name,
        description: this.formData.description,
        duration: durationMinutes,
        durationMinutes: durationMinutes,
        price: numericValue,
        category: this.formData.category || 'Cabelo',
        active: true,
        barbershopId 
      };
      
      if (this.isEditing) {
        await api.put(`/services_types/${this.formData.id}?barbershopId=${barbershopId}`, payload);
      } else {
        await api.post(`/services_types?barbershopId=${barbershopId}`, payload);
      }
      
      this.toastr.success('Serviço salvo com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadServices();
    } catch (err) {
      this.globalService.errorNotification(err);
      this.toastr.error('Erro ao salvar serviço. Tente novamente.', 'Erro');
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }

  openDeleteModal(srv: any) {
    this.serviceToDelete = srv;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.serviceToDelete = null;
  }

  async confirmDelete() {
    if (this.isDeleting || !this.serviceToDelete) return;
    this.isDeleting = true;
    this.cdr.detectChanges();
    try {
      await api.delete(`/services_types/${this.serviceToDelete.id}`);
      
      this.toastr.success('Serviço excluído com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadServices();
    } catch (err) {
      console.error('Erro ao excluir serviço', err);
      this.toastr.error('Erro ao excluir serviço. Tente novamente.', 'Erro');
    } finally {
      this.isDeleting = false;
      this.cdr.detectChanges();
    }
  }
}
