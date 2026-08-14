import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-barbers',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective],
  templateUrl: './barbers.html',
  styleUrls: ['./barbers.css']
})
export class Barbers implements OnInit {
  barbers: any[] = [];
  loading = true;
  
  isModalOpen = false;
  isDeleteModalOpen = false;
  isEditing = false;
  barberToDelete: any = null;
  
  formData = {
    id: '',
    name: '',
    email: '',
    whatsApp: '',
    document: ''
  };

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadBarbers();
  }

  async loadBarbers() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      const response = await api.get(`/users?role=Barber&barbershopId=${barbershopId}`);
      this.barbers = response.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar profissionais', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  openModal(barber?: any) {
    if (barber) {
      this.isEditing = true;
      this.formData = {
        id: barber.id,
        name: barber.name,
        email: barber.email,
        whatsApp: barber.whatsApp || '',
        document: barber.document || ''
      };
    } else {
      this.isEditing = false;
      this.formData = { id: '', name: '', email: '', whatsApp: '', document: '' };
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
    const doc = this.formData.document ? this.formData.document.toString() : '';
    
    const emailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    const phoneValid = phone.replace(/\D/g, '').length >= 10;
    const nameValid = name.trim().length >= 2;
    const docValid = doc.replace(/\D/g, '').length === 11 || doc === '';
    return emailValid && phoneValid && nameValid && docValid;
  }

  async saveBarber() {
    if (!this.isFormValid()) {
      this.toastr.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      const payload: any = {
        ...this.formData,
        role: 1, 
        barbershopId: barbershopId
      };

      if (!payload.id) {
        delete payload.id;
      }

      if (this.isEditing) {
        await api.put(`/users/${this.formData.id}?barbershopId=${barbershopId}`, payload);
      } else {
        await api.post(`/users?barbershopId=${barbershopId}`, payload);
      }
      
      this.toastr.success('Profissional salvo com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadBarbers();
    } catch (err) {
      console.error('Erro ao salvar profissional', err);
      this.toastr.error('Erro ao salvar profissional. Tente novamente.', 'Erro');
    } finally {
      this.cdr.detectChanges();
    }
  }

  openDeleteModal(barber: any) {
    this.barberToDelete = barber;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.barberToDelete = null;
  }

  async confirmDelete() {
    if (!this.barberToDelete) return;
    
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      await api.delete(`/users/${this.barberToDelete.id}?barbershopId=${barbershopId}`);
      
      this.toastr.success('Profissional excluído com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadBarbers();
    } catch (err) {
      console.error('Erro ao excluir profissional', err);
      this.toastr.error('Erro ao excluir profissional. Tente novamente.', 'Erro');
    } finally {
      this.cdr.detectChanges();
    }
  }
}
