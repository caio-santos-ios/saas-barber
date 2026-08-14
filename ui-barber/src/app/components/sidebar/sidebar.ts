import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { Router } from '@angular/router';
import { Auth } from '../../services/auth';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css']
})
export class Sidebar {
  constructor(private auth: Auth, private router: Router) {}

  logout() {
    this.auth.clearToken();
    this.router.navigate(['/login']);
  }
}
