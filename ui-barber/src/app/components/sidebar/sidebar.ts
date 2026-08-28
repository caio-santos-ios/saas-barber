import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { Router } from '@angular/router';
import { Auth } from '../../services/auth';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css']
})
export class Sidebar {
  constructor(private auth: Auth, private router: Router, public themeService: ThemeService) {}

  logout() {
    this.auth.clearToken();
    this.router.navigate(['/login']);
  }
}
