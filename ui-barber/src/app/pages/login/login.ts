import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../services/auth';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class Login {
  email = '';
  password = '';
  loading = false;
  showPassword = false;

  isResetModalOpen = false;
  resetEmail = '';
  resetLoading = false;

  constructor(
    private auth: Auth,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  openResetModal() {
    this.resetEmail = '';
    this.isResetModalOpen = true;
  }

  closeResetModal() {
    this.isResetModalOpen = false;
  }

  async submitReset() {
    if (!this.resetEmail) {
      this.toastr.warning('Informe seu e-mail.', 'Atenção');
      return;
    }
    this.resetLoading = true;
    try {
      await api.post('/auth/reset-password', { email: this.resetEmail, originUrl: window.location.origin, role: "Admin" });
      this.closeResetModal();
      this.toastr.success(
        'Uma nova senha temporária foi enviada para o seu e-mail.',
        'E-mail Enviado',
        { timeOut: 8000 }
      );
    } catch (err: any) {
      const msg = err.response?.data?.message || 'E-mail não encontrado.';
      this.toastr.error(msg, 'Erro');
    } finally {
      this.resetLoading = false;
      this.cdr.detectChanges();
    }
  }

  async onSubmit() {
    if (!this.email || !this.password) {
      this.toastr.warning('Por favor, preencha e-mail e senha.', 'Atenção');
      return;
    }

    this.loading = true;
    try {
      const response = await api.post('/auth/login', { email: this.email, password: this.password, role: "Admin" });
      const payload = response.data.data;
      this.auth.setToken(payload.token);
      this.auth.setSubscriptionStatus(payload.subscriptionStatus);
      localStorage.setItem('barbershopId', payload.barbershopId);
      
      this.toastr.success('Login realizado com sucesso!', 'Bem-vindo');
      
      if (this.auth.hasActiveSubscription()) {
        this.router.navigate(['/dashboard']);
      } else {
        this.toastr.warning('Sua assinatura está inativa. Regularize para acessar o sistema.', 'Assinatura Inativa');
        this.router.navigate(['/subscription']);
      }
    } catch (err: any) {
      console.error(err);
      const apiError = err.response?.data?.error || err.response?.data?.title;
      if (apiError) {
        this.toastr.error(`Erro na API: ${apiError}`, 'Falha de Login');
      } else {
        this.toastr.error('Credenciais inválidas ou erro ao conectar com o servidor.', 'Erro de Login');
      }
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }
}

