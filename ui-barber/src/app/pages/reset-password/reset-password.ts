import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reset-password.html',
  styleUrls: ['./reset-password.css']
})
export class ResetPassword implements OnInit {
  password = '';
  confirmPassword = '';
  code = '';
  loading = false;
  showPassword = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.code = params['code'] || '';
      if (!this.code) {
        this.toastr.error('Link inválido ou expirado.', 'Erro');
        this.router.navigate(['/login']);
      }
    });
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  async onSubmit() {
    if (!this.password || !this.confirmPassword) {
      this.toastr.warning('Por favor, preencha as senhas.', 'Atenção');
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.toastr.warning('As senhas não coincidem.', 'Atenção');
      return;
    }

    this.loading = true;
    try {
      await api.post('/auth/confirm-reset-password', { 
        code: this.code, 
        newPassword: this.password 
      });
      
      this.toastr.success('Sua senha foi redefinida com sucesso!', 'Sucesso');
      this.router.navigate(['/login']);
    } catch (err: any) {
      console.error(err);
      const apiError = err.response?.data?.message || 'Ocorreu um erro ao redefinir a senha.';
      this.toastr.error(apiError, 'Falha');
    } finally {
      this.loading = false;
    }
  }
}
