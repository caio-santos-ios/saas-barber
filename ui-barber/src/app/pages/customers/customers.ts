import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';
import { ChangeDetectorRef } from '@angular/core';

import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective, RouterLink],
  templateUrl: './customers.html',
  styleUrls: ['./customers.css']
})
export class Customers implements OnInit {
  customers: any[] = [];
  loading = true;

  isModalOpen = false;
  isDeleteModalOpen = false;
  isEditing = false;
  customerToDelete: any = null;
  
  formData = {
    id: '',
    name: '',
    email: '',
    whatsApp: ''
  };

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadCustomers();
  }

  async loadCustomers() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const response = await api.get(`/users/customers?deleted=false`);
      this.customers = response.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar clientes', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  openModal(customer?: any) {
    if (customer) {
      this.isEditing = true;
      this.formData = {
        id: customer.id,
        name: customer.name,
        email: customer.email,
        whatsApp: customer.whatsApp || ''
      };
    } else {
      this.isEditing = false;
      this.formData = { id: '', name: '', email: '', whatsApp: '' };
    }
    this.isModalOpen = true;
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
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    try {
      const payload: any = {
        ...this.formData,
        role: 2
      };

      if (!payload.id) {
        delete payload.id;
      }

      if (this.isEditing) {
        await api.put(`/users/${this.formData.id}`, payload);
      } else {
        await api.post(`/auth/customers/register`, payload);
      }
      
      this.toastr.success('Cliente salvo com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadCustomers();
    } catch (err) {
      this.toastr.error('Erro ao salvar cliente. Tente novamente.', 'Erro');
    } finally {
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
    if (!this.customerToDelete) return;
    
    try {
      await api.delete(`/users/${this.customerToDelete.id}`);
      this.toastr.success('Cliente excluído com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadCustomers();
    } catch (err) {
      this.toastr.error('Erro ao excluir cliente. Tente novamente.', 'Erro');
    } finally {
      this.cdr.detectChanges();
    }
  }
}
