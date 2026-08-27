import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class GlobalService {
  protected toastrNotification = inject(ToastrService);
  protected router = inject(Router);

  errorNotification(err: any) {
    if (!err) return;

    const status = err.response?.status ?? err.status;
    const message =
      err.response?.data?.message ??
      err.response?.data?.Message ??
      err.message ??
      'Ocorreu um erro inesperado. Por favor, tente novamente.';

    if (status === 401) {
      this.toastrNotification.warning('Sessão finalizada', 'Atenção');
      localStorage.clear();
      this.router.navigate(['/login']);
      return;
    }

    if (status >= 400 && status < 500) {
      this.toastrNotification.warning(message, 'Atenção');
    } else {
      this.toastrNotification.error(message, 'Erro');
    }
  }
}
