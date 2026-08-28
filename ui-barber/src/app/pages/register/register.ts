import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../services/auth';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, NgxMaskDirective],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
  formData = {
    name: '',
    barbershopName: '',
    typePerson: 'PF',
    document: '',
    whatsApp: '',
    email: '',
    password: '',
    passwordConfirm: '',
    acceptTerms: false,
    firebaseUid: ''
  };
  
  loading = false;
  showPassword = false;
  showPasswordConfirm = false;

  constructor(
    private auth: Auth, 
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  togglePasswordConfirm() {
    this.showPasswordConfirm = !this.showPasswordConfirm;
  }

  get documentMask(): string {
    return this.formData.typePerson === 'PF' ? '000.000.000-00' : '00.000.000/0000-00';
  }

  async onSubmit() {
    if (!this.formData.name || !this.formData.barbershopName || !this.formData.document || !this.formData.whatsApp || !this.formData.email || !this.formData.password || !this.formData.passwordConfirm) {
      this.toastr.warning('Por favor, preencha todos os campos obrigatórios.', 'Atenção');
      return;
    }

    if (this.formData.password !== this.formData.passwordConfirm) {
      this.toastr.warning('As senhas não coincidem.', 'Atenção');
      return;
    }

    if (!this.formData.acceptTerms) {
      this.toastr.warning('Você precisa aceitar os termos e condições para continuar.', 'Atenção');
      return;
    }

    this.loading = true;
    this.cdr.detectChanges();

    try {
      const payload = {
        name: this.formData.name,
        barbershopName: this.formData.barbershopName,
        typePerson: this.formData.typePerson,
        document: this.formData.document.replace(/\D/g, ''),
        whatsApp: this.formData.whatsApp.replace(/\D/g, ''),
        email: this.formData.email,
        password: this.formData.password
      };
      await api.post('/auth/admins/register', payload);
      this.toastr.success('Cadastro realizado com sucesso!', 'Sucesso');
      setTimeout(() => this.router.navigate(['/login']), 2000);
    } catch (err: any) {
      console.error(err);
      const apiError = err.response?.data?.message || err.response?.data?.error || err.response?.data?.title;
      if (apiError) {
         this.toastr.error(`Erro no cadastro: ${apiError}`, 'Falha no Cadastro');
      } else {
         this.toastr.error('Erro ao processar cadastro na API.', 'Erro');
      }
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }
}
