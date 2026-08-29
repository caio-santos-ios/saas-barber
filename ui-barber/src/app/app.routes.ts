import { Routes } from '@angular/router';
import { DashboardLayout } from './layouts/dashboard-layout/dashboard-layout';
import { Dashboard } from './pages/dashboard/dashboard';
import { Agenda } from './pages/agenda/agenda';
import { Customers } from './pages/customers/customers';
import { Barbers } from './pages/barbers/barbers';
import { Services } from './pages/services/services';
import { Schedules } from './pages/schedules/schedules';
import { Subscription } from './pages/subscription/subscription';
import { Settings } from './pages/settings/settings';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { ResetPassword } from './pages/reset-password/reset-password';
import { ConfirmEmail } from './pages/confirm-email/confirm-email';
import { ShareApp } from './pages/share-app/share-app';
import { AppLanding } from './pages/app-landing/app-landing';
import { AuthGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'reset-password', component: ResetPassword },
  { path: 'confirm-email', component: ConfirmEmail },
  { path: 'confirm-account', component: ConfirmEmail },
  { path: 'register', component: Register },
  { path: 'app/:barbershopId', component: AppLanding },
  { path: 'baixar', component: AppLanding },
  {
    path: '',
    component: DashboardLayout,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'subscription', component: Subscription },
      { path: 'calendar', component: Agenda },
      { path: 'customers', component: Customers },
      { path: 'barbers', component: Barbers },
      { path: 'services', component: Services },
      { path: 'schedules', component: Schedules },
      { path: 'share', component: ShareApp },
      { path: 'settings', component: Settings }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
