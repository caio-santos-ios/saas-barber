import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';
import { ChangeDetectorRef } from '@angular/core';
import { GlobalService } from '../../services/global.service';

@Component({
  selector: 'app-barbers',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective],
  templateUrl: './barbers.html',
  styleUrls: ['./barbers.css']
})
export class Barbers extends GlobalService implements OnInit {
  barbers: any[] = [];
  loading = true;
  
  isModalOpen = false;
  isDeleteModalOpen = false;
  isPasswordModalOpen = false;
  isEditing = false;
  isSaving = false;
  isDeleting = false;
  isChangingPassword = false;
  barberToDelete: any = null;
  barberToChangePassword: any = null;
  newPassword = '';
  showPassword = false;
  showNewPassword = false;
  
  formData = {
    id: '',
    name: '',
    email: '',
    whatsApp: '',
    document: '',
    password: ''
  };

  constructor(
    private cdr: ChangeDetectorRef
  ) {
    super();
  }

  ngOnInit() {
    this.loadBarbers();
  }

  async loadBarbers() {
    this.spinnerShow();
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId') || '';
      const response = await api.get(`/users/barbers?deleted=false&barbershopId=${barbershopId}`);
      this.barbers = response.data.data || [];
    } catch (err) {
      this.errorNotification(err);
      console.error('Erro ao carregar profissionais', err);
    } finally {
      this.loading = false;
      this.spinnerHide();
      this.cdr.detectChanges();
    }
  }

  async openModal(barber?: any) {
    this.showPassword = false;
    const barberId = typeof barber === 'string' ? barber : barber?.id;
    if (barberId) {
      this.isEditing = true;
      await this.getById(barberId);
    } else {
      this.isModalOpen = true;
      this.isEditing = false;
      this.formData = { id: '', name: '', email: '', whatsApp: '', document: '', password: '' };
    }
  }

  async getById(id: string) {
    try {
      this.isModalOpen = true;
      const response = await api.get(`/users/${id}`);
      const data = response.data.data;
      
      this.formData = {
        id: data.id,
        name: data.name || '',
        email: data.email || '',
        whatsApp: data.whatsApp || '',
        document: data.document || '',
        password: ''
      };
      this.cdr.detectChanges();
    } catch (error) {
      this.toastrNotification.error('Erro ao buscar dados do profissional', 'Erro');
    }
  }

  closeModal() {
    this.isModalOpen = false;
    this.showPassword = false;
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
    if (this.isSaving) return;
    if (!this.isFormValid()) {
      this.toastrNotification.warning('Preencha todos os campos obrigatórios corretamente.', 'Atenção');
      return;
    }
    this.isSaving = true;
    this.cdr.detectChanges();
    try {
      const payload: any = {
        ...this.formData,
        role: 'Barber'
      };

      if (!payload.id) {
        delete payload.id;
      }

      if (this.isEditing) {
        delete payload.password;
        await api.put(`/users`, payload);
      } else {
        await api.post(`/users`, payload);
      }
      
      this.toastrNotification.success('Profissional salvo com sucesso!', 'Sucesso');
      this.closeModal();
      await this.loadBarbers();
    } catch (err) {
      console.error('Erro ao salvar profissional', err);
      this.toastrNotification.error('Erro ao salvar profissional. Tente novamente.', 'Erro');
    } finally {
      this.isSaving = false;
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
    if (this.isDeleting || !this.barberToDelete) return;
    this.isDeleting = true;
    this.cdr.detectChanges();
    try {
      await api.delete(`/users/${this.barberToDelete.id}`);
      
      this.toastrNotification.success('Profissional excluído com sucesso!', 'Sucesso');
      this.closeDeleteModal();
      await this.loadBarbers();
    } catch (err) {
      console.error('Erro ao excluir profissional', err);
      this.toastrNotification.error('Erro ao excluir profissional. Tente novamente.', 'Erro');
    } finally {
      this.isDeleting = false;
      this.cdr.detectChanges();
    }
  }

  openPasswordModal(barber: any) {
    this.barberToChangePassword = barber;
    this.newPassword = '';
    this.showNewPassword = false;
    this.isPasswordModalOpen = true;
  }

  closePasswordModal() {
    this.isPasswordModalOpen = false;
    this.barberToChangePassword = null;
    this.newPassword = '';
    this.showNewPassword = false;
  }

  async savePassword() {
    if (this.isChangingPassword) return;
    if (this.newPassword.length < 6) {
      this.toastrNotification.warning('A senha deve ter pelo menos 6 caracteres.', 'Atenção');
      return;
    }
    this.isChangingPassword = true;
    this.cdr.detectChanges();
    try {
      await api.patch(`/users/${this.barberToChangePassword.id}/password`, { password: this.newPassword });
      this.toastrNotification.success('Senha alterada com sucesso!', 'Sucesso');
      this.closePasswordModal();
    } catch (err) {
      console.error('Erro ao alterar senha', err);
      this.toastrNotification.error('Erro ao alterar senha. Tente novamente.', 'Erro');
    } finally {
      this.isChangingPassword = false;
      this.cdr.detectChanges();
    }
  }
}
