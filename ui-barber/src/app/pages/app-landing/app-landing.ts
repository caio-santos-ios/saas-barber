import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { api } from '../../services/api';

@Component({
  selector: 'app-landing-customer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app-landing.html',
  styleUrls: ['./app-landing.css']
})
export class AppLanding implements OnInit {
  barbershopId = '';
  barbershopName = 'Barbearia';
  barbershopPhone = '';
  barbershopAddress = '';
  barbershopLogo = '';
  
  loading = true;
  deepLinkUrl = '';

  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    this.route.params.subscribe(async params => {
      this.barbershopId = params['barbershopId'] || this.route.snapshot.queryParams['barbershopId'] || '';
      this.deepLinkUrl = `cortae://register?barbershopId=${this.barbershopId}`;
      await this.loadBarbershop();
    });
  }

  async loadBarbershop() {
    if (!this.barbershopId) {
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }

    try {
      const res = await api.get(`/barbershops/${this.barbershopId}`);
      const data = res.data?.data;
      if (data) {
        this.barbershopName = data.name || this.barbershopName;
        this.barbershopPhone = data.phone || '';
        this.barbershopLogo = data.logo || '';
        if (data.address) {
          const parts = [
            data.address.street ? `${data.address.street}, ${data.address.number || 'S/N'}` : '',
            data.address.neighborhood || '',
            data.address.city ? `${data.address.city} - ${data.address.state || ''}` : ''
          ].filter(p => p.length > 0);
          this.barbershopAddress = parts.join(' - ');
        }
      }
    } catch (err) {
      console.error('Erro ao carregar barbearia', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  openAppOrStore(store: 'play' | 'apple' | 'web') {
    // Attempt deep link if on mobile
    if (store === 'play') {
      window.open('https://play.google.com/store', '_blank');
    } else if (store === 'apple') {
      window.open('https://www.apple.com/app-store/', '_blank');
    } else {
      // Direct deep link
      window.location.href = this.deepLinkUrl;
    }
  }
}
