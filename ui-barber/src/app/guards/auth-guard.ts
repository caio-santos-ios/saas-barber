import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Auth } from '../services/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private auth: Auth, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.auth.isAuthenticated()) {
      // Allow access to subscription screen regardless of plan status
      if (state.url.includes('/subscription')) {
        return true;
      }

      // For all other authenticated routes, check subscription status
      if (!this.auth.hasActiveSubscription()) {
        this.router.navigate(['/subscription']);
        return false;
      }

      return true;
    }
    
    this.router.navigate(['/login']);
    return false;
  }
}
