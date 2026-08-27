import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { api } from '../../services/api';
import QRCode from 'qrcode';

@Component({
  selector: 'app-share-app',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './share-app.html',
  styleUrls: ['./share-app.css']
})
export class ShareApp implements OnInit {
  barbershopId = '';
  barbershopCode = '';
  barbershopName = 'Nossa Barbearia';
  barbershopPhone = '';
  barbershopAddress = '';
  
  appUrl = '';
  qrCodeDataUrl = '';
  loading = true;

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    this.barbershopId = localStorage.getItem('barbershopId') || '';
    const origin = window.location.origin;
    this.appUrl = `${origin}/app/${this.barbershopId}`;

    await this.loadBarbershopInfo();
    await this.generateQrCode();
  }

  async loadBarbershopInfo() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      if (this.barbershopId) {
        const response = await api.get(`/barbershops/${this.barbershopId}`);
        const shop = response.data?.data;
        if (shop) {
          this.barbershopName = shop.name || this.barbershopName;
          this.barbershopCode = shop.code || '';
          this.barbershopPhone = shop.phone || '';
          if (shop.address) {
            const parts = [
              shop.address.street ? `${shop.address.street}, ${shop.address.number || 'S/N'}` : '',
              shop.address.neighborhood || '',
              shop.address.city ? `${shop.address.city} - ${shop.address.state || ''}` : ''
            ].filter(p => p.length > 0);
            this.barbershopAddress = parts.join(' - ');
          }
        }
      }
    } catch (err) {
      console.error('Erro ao carregar dados da barbearia', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  async generateQrCode() {
    try {
      const qrContent = this.barbershopCode || this.barbershopId;
      this.qrCodeDataUrl = await QRCode.toDataURL(qrContent, {
        width: 320,
        margin: 2,
        color: {
          dark: '#111827',
          light: '#ffffff'
        },
        errorCorrectionLevel: 'H'
      });
      this.cdr.detectChanges();
    } catch (err) {
      console.error('Erro ao gerar QR Code', err);
    }
  }

  copyCode() {
    if (!this.barbershopCode) return;
    navigator.clipboard.writeText(this.barbershopCode).then(() => {
      this.toastr.success('Código da barbearia copiado!', 'Sucesso');
    }).catch(() => {
      this.toastr.error('Não foi possível copiar o código.', 'Erro');
    });
  }

  copyLink() {
    navigator.clipboard.writeText(this.appUrl).then(() => {
      this.toastr.success('Link copiado para a área de transferência!', 'Sucesso');
    }).catch(() => {
      this.toastr.error('Não foi possível copiar o link.', 'Erro');
    });
  }

  shareWhatsApp() {
    const text = encodeURIComponent(
      `Olá! Agora você pode agendar seu horário na *${this.barbershopName}* direto pelo nosso aplicativo!\n\n` +
      `Baixe o app e conecte-se usando o nosso código exclusivo: *${this.barbershopCode}*\n\n` +
      `Esperamos por você!`
    );
    window.open(`https://api.whatsapp.com/send?text=${text}`, '_blank');
  }

  printPoster() {
    window.print();
  }

  downloadQrCode() {
    if (!this.qrCodeDataUrl) return;
    const a = document.createElement('a');
    a.href = this.qrCodeDataUrl;
    a.download = `qrcode-${(this.barbershopCode || this.barbershopName).toLowerCase().replace(/\s+/g, '-')}.png`;
    a.click();
    this.toastr.info('Download do QR Code iniciado!', 'Download');
  }
}
