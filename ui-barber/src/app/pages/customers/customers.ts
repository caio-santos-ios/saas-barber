import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';
import { ChangeDetectorRef } from '@angular/core';

import { RouterLink } from '@angular/router';
import { GlobalService } from '../../services/global.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective, RouterLink],
  templateUrl: './customers.html',
  styleUrls: ['./customers.css']
})
export class Customers extends GlobalService implements OnInit {
  customers: any[] = [];
  loading = true;

  isModalOpen = false;
  isDeleteModalOpen = false;
  isEditing = false;
  isSaving = false;
  isDeleting = false;
  customerToDelete: any = null;
  
  formData = {
    id: '',
    name: '',
    email: '',
    whatsApp: ''
  };

  constructor(
    private cdr: ChangeDetectorRef
  ) {
    super();
  }

  ngOnInit() {
    this.loadCustomers();
  }

  async loadCustomers() {
    this.spinnerShow();
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId') || '';
      const response = await api.get(`/users/customers?deleted=false&barbershopId=${barbershopId}`);
      this.customers = response.data.data || [];
    } catch (err) {
      this.errorNotification(err);
      console.error('Erro ao carregar clientes', err);
    } finally {
      this.loading = false;
      this.spinnerHide();
      this.cdr.detectChanges();
    }
  }

  async openModal(customer?: any) {
    const customerId = typeof customer === 'string' ? customer : customer?.id;
    if (customerId) {
      this.isEditing = true;
      await this.getById(customerId);
    } else {
      this.isModalOpen = true;
      this.isEditing = false;
      this.formData = { id: '', name: '', email: '', whatsApp: '' };
    }
  }

  async getById(id: string) {
    try {
      this.spinnerShow();
      this.isModalOpen = true;
      const response = await api.get(`/users/${id}`);
      const data = response.data.data;
      this.formData = {
        id: data.id,
        name: data.name || '',
        email: data.email || '',
        whatsApp: data.whatsapp || data.whatsApp || ''
      };
      this.cdr.detectChanges();
    } catch (error) {
      this.toastr.error('Erro ao buscar dados do cliente', 'Erro');
    } finally {
      this.spinnerHide();
    }
  }

  closeModal() {
    this.isModalOpen = false;
  }

  isFormValid(): boolean {
    const email = this.formData.email || '';
    const name = this.formData.name || '';
    const phone = this.formData.whatsApp ? this.formData.whatsApp.toString() : '';
    
    const emailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    const phoneValid = phone.replace(/\D/g, '').length >= 10;
    const nameValid = name.trim().length >= 2;
    return emailValid && phoneValid && nameValid;
  }

  async saveCustomer() {
    if (this.isSaving) return;
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    this.isSaving = true;
    this.cdr.detectChanges();
    try {
      const payload: any = {
        ...this.formData,
        role: 2
      };

      if (!payload.id) {
        delete payload.id;
      }

      if (this.isEditing) {
        await api.put(`/users`, payload);
      } else {
        await api.post(`/auth/customers/register`, payload);
      }
      
      this.toastr.success('Cliente salvo com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadCustomers();
    } catch (err) {
      this.toastr.error('Erro ao salvar cliente. Tente novamente.', 'Erro');
    } finally {
      this.isSaving = false;
      this.cdr.detectChanges();
    }
  }

  openDeleteModal(customer: any) {
    this.customerToDelete = customer;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.customerToDelete = null;
  }

  async confirmDelete() {
    if (this.isDeleting || !this.customerToDelete) return;
    this.isDeleting = true;
    this.cdr.detectChanges();
    try {
      await api.delete(`/users/${this.customerToDelete.id}`);
      this.toastr.success('Cliente excluído com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadCustomers();
    } catch (err) {
      this.toastr.error('Erro ao excluir cliente. Tente novamente.', 'Erro');
    } finally {
      this.isDeleting = false;
      this.cdr.detectChanges();
    }
  }
}
