import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-email.html',
  styleUrls: ['./confirm-email.css']
})
export class ConfirmEmail implements OnInit {
  code = '';
  state: 'loading' | 'success' | 'error' = 'loading';
  errorMessage = '';
  userRole = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(async (params) => {
      this.code = params['code'] || params['token'] || '';
      if (!this.code) {
        this.state = 'error';
        this.errorMessage = 'Link de confirmação inválido ou ausente. Verifique o link recebido no seu e-mail.';
        this.cdr.detectChanges();
        return;
      }

      await this.confirmAccount();
    });
  }

  async confirmAccount() {
    this.state = 'loading';
    this.cdr.detectChanges();

    try {
      const response = await api.post('/auth/confirm-email', { code: this.code });
      this.userRole = response.data?.data?.role || response.data?.role || '';
      this.state = 'success';
      this.toastr.success('Sua conta foi confirmada e ativada com sucesso!', 'Bem-vindo');
      this.cdr.detectChanges();

      if (this.userRole.toLowerCase() === 'admin') {
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }
    } catch (err: any) {
      this.state = 'error';
      this.errorMessage = err.response?.data?.message || 'Link de confirmação inválido ou expirado. Por favor, solicite um novo link ou entre em contato com o suporte.';
      this.cdr.detectChanges();
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
