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
  isPasswordModalOpen = false;
  isEditing = false;
  barberToDelete: any = null;
  barberToChangePassword: any = null;
  newPassword = '';
  
  formData = {
    id: '',
    name: '',
    email: '',
    whatsApp: '',
    document: '',
    password: ''
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
      const response = await api.get(`/users/barbers`);
      console.log(response.data.data)
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
        document: barber.document || '',
        password: ''
      };
    } else {
      this.isEditing = false;
      this.formData = { id: '', name: '', email: '', whatsApp: '', document: '', password: '' };
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
    const passwordValid = this.isEditing || this.formData.password.length >= 6;
    return emailValid && phoneValid && nameValid && docValid && passwordValid;
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
        role: 'Barber',
        barbershopId: barbershopId
      };

      if (!payload.id) {
        delete payload.id;
      }

      if (this.isEditing) {
        delete payload.password;
        await api.put(`/users/${this.formData.id}?barbershopId=${barbershopId}`, payload);
      } else {
        if (payload.password) {
          payload.password = payload.password;
        }
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

  openPasswordModal(barber: any) {
    this.barberToChangePassword = barber;
    this.newPassword = '';
    this.isPasswordModalOpen = true;
  }

  closePasswordModal() {
    this.isPasswordModalOpen = false;
    this.barberToChangePassword = null;
    this.newPassword = '';
  }

  async savePassword() {
    if (this.newPassword.length < 6) {
      this.toastr.warning('A senha deve ter pelo menos 6 caracteres.', 'Atenção');
      return;
    }
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      await api.patch(`/users/${this.barberToChangePassword.id}/password?barbershopId=${barbershopId}`, { password: this.newPassword });
      this.toastr.success('Senha alterada com sucesso!', 'Sucesso');
      this.closePasswordModal();
    } catch (err) {
      console.error('Erro ao alterar senha', err);
      this.toastr.error('Erro ao alterar senha. Tente novamente.', 'Erro');
    } finally {
      this.cdr.detectChanges();
    }
  }
}
