import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class GlobalService {
  protected toastr = inject(ToastrService);
  protected toastrNotification = this.toastr;
  protected router = inject(Router);

  static isLoading = signal<boolean>(false);

  get loadingState() {
    return GlobalService.isLoading();
  }

  spinnerShow() {
    GlobalService.isLoading.set(true);
  }
  
  spinnerHide() {
    GlobalService.isLoading.set(false);
  }

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
      const theme = localStorage.getItem('theme');
      localStorage.clear();
      if (theme) localStorage.setItem('theme', theme);
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
