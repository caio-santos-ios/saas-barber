import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective],
  templateUrl: './settings.html',
  styleUrls: []
})
export class Settings implements OnInit {
  loading = true;
  saving = false;
  
  formData = {
    id: '',
    name: '',
    document: '',
    phone: '',
    address: {
      zipCode: '',
      street: '',
      number: '',
      complement: '',
      neighborhood: '',
      city: '',
      state: ''
    }
  };

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadSettings();
  }

  async loadSettings() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      
      const response = await api.get(`/barbershops/${barbershopId}`);
      const shop = response.data?.data;
      if (shop) {
        this.formData = {
          id: shop.id || shop.Id || barbershopId || '',
          name: shop.name || shop.Name || '',
          document: shop.document || shop.Document || '',
          phone: shop.phone || shop.Phone || shop.whatsApp || shop.WhatsApp || '',
          address: {
            zipCode: shop.address?.zipCode || shop.address?.ZipCode || '',
            street: shop.address?.street || shop.address?.Street || '',
            number: shop.address?.number || shop.address?.Number || '',
            complement: shop.address?.complement || shop.address?.Complement || '',
            neighborhood: shop.address?.neighborhood || shop.address?.Neighborhood || '',
            city: shop.address?.city || shop.address?.City || '',
            state: shop.address?.state || shop.address?.State || ''
          }
        };
      }
    } catch (err) {
      console.error('Erro ao carregar configurações', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  async searchCep() {
    const cep = this.formData.address.zipCode?.replace(/\D/g, '');
    if (cep?.length === 8) {
      try {
        const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
        const data = await response.json();
        if (!data.erro) {
          this.formData.address.street = data.logradouro || '';
          this.formData.address.neighborhood = data.bairro || '';
          this.formData.address.city = data.localidade || '';
          this.formData.address.state = data.uf || '';
          this.cdr.detectChanges();
        }
      } catch (err) {
        console.error('Erro ao buscar CEP', err);
      }
    }
  }

  async saveSettings() {
    if (!this.formData.id) return;
    this.saving = true;
    this.cdr.detectChanges();
    
    try {
      const barbershopId = localStorage.getItem('barbershopId');

      const payload = {
        name: this.formData.name,
        document: this.formData.document,
        phone: this.formData.phone,
        address: {
          street: this.formData.address.street,
          number: this.formData.address.number,
          complement: this.formData.address.complement,
          neighborhood: this.formData.address.neighborhood,
          city: this.formData.address.city,
          state: this.formData.address.state,
          zipCode: this.formData.address.zipCode,
          country: "Brasil"
        }
      };

      await api.put(`/barbershops/${this.formData.id}?barbershopId=${barbershopId}`, payload);
      this.toastr.success('Configurações atualizadas com sucesso!', 'Sucesso');
    } catch (err) {
      console.error('Erro ao salvar configurações', err);
      this.toastr.error('Falha ao salvar as configurações. Verifique os dados.', 'Erro');
    } finally {
      this.saving = false;
      this.cdr.detectChanges();
    }
  }
}
