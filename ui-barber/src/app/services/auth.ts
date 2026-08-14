import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  setToken(token: string) {
    if (this.isBrowser) {
      localStorage.setItem('token', token);
    }
  }

  setSubscriptionStatus(status: string) {
    if (this.isBrowser) {
      localStorage.setItem('subscriptionStatus', status);
    }
  }

  getSubscriptionStatus(): string | null {
    if (this.isBrowser) {
      return localStorage.getItem('subscriptionStatus');
    }
    return null;
  }

  hasActiveSubscription(): boolean {
    const status = this.getSubscriptionStatus();
    return status === 'Ativa' || status === 'Active';
  }

  getToken(): string | null {
    return this.isBrowser ? localStorage.getItem('token') : null;
  }

  clearToken() {
    localStorage.removeItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

}
