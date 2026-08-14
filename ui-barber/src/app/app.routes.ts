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
import { AuthGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
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
      { path: 'settings', component: Settings }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
