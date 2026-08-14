import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { ChangeDetectorRef } from '@angular/core';
import { NgxMaskDirective } from 'ngx-mask';

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
  serviceToDelete: any = null;
  
  formData = {
    id: '',
    name: '',
    description: '',
    durationMinutes: 30,
    value: 0,
    category: ''
  };

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadServices();
  }

  async loadServices() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      const response = await api.get(`/services_types?barbershopId=${barbershopId}`);
      this.services = response.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar serviços', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  openModal(srv?: any) {
    if (srv) {
      this.isEditing = true;
      this.formData = {
        id: srv.id,
        name: srv.name,
        description: srv.description || '',
        durationMinutes: srv.durationMinutes || srv.duration || 30,
        value: srv.value || 0,
        category: srv.category || 'Cabelo'
      };
    } else {
      this.isEditing = false;
      this.formData = { id: '', name: '', description: '', durationMinutes: 30, value: 0, category: 'Cabelo' };
    }
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  getParsedValue(): number {
    const stringValue = (this.formData.value || 0).toString();
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

  async saveService() {
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      
      const numericValue = this.getParsedValue();

      const payload: any = { 
        ...this.formData, 
        value: numericValue,
        barbershopId 
      };
      
      if (!payload.id) {
        delete payload.id;
      }

      if (this.isEditing) {
        await api.put(`/services_types/${this.formData.id}?barbershopId=${barbershopId}`, payload);
      } else {
        await api.post(`/services_types?barbershopId=${barbershopId}`, payload);
      }
      
      this.toastr.success('Serviço salvo com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadServices();
    } catch (err) {
      console.error('Erro ao salvar serviço', err);
      this.toastr.error('Erro ao salvar serviço. Tente novamente.', 'Erro');
    } finally {
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
    if (!this.serviceToDelete) return;
    
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      await api.delete(`/services_types/${this.serviceToDelete.id}?barbershopId=${barbershopId}`);
      
      this.toastr.success('Serviço excluído com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadServices();
    } catch (err) {
      console.error('Erro ao excluir serviço', err);
      this.toastr.error('Erro ao excluir serviço. Tente novamente.', 'Erro');
    } finally {
      this.cdr.detectChanges();
    }
  }
}
