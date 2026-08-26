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
    
    // Generate public customer app link
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
      this.qrCodeDataUrl = await QRCode.toDataURL(this.appUrl, {
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
      `Acesse o link para baixar o app e agendar em poucos segundos:\n${this.appUrl}\n\n` +
      `Esperamos você!`
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
    a.download = `qrcode-${this.barbershopName.toLowerCase().replace(/\s+/g, '-')}.png`;
    a.click();
    this.toastr.info('Download do QR Code iniciado!', 'Download');
  }
}
