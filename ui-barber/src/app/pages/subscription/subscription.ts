import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-subscription',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective],
  templateUrl: './subscription.html',
  styleUrls: ['./subscription.css']
})
export class Subscription implements OnInit {
  plans: any[] = [];
  loading = true;
  processing = false;
  isCancelModalOpen = false;
  
  currentPlanId = '';

  paymentData = {
    holderName: '',
    number: '',
    expiryMonth: '',
    expiryYear: '',
    ccv: ''
  };

  selectedPlan: any = null;

  invoices: any[] = [];

  constructor(
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef,
    public auth: Auth,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadPlans();
  }

  async loadPlans() {
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      
      const response = await api.get(`/plans?barbershopId=${barbershopId}`);
      this.plans = response.data.data || [];

      const shopRes = await api.get(`/barbershops?barbershopId=${barbershopId}`);
      if (shopRes.data.data && shopRes.data.data.length > 0) {
        this.currentPlanId = shopRes.data.data[0].planId;
      }

      if (this.auth.hasActiveSubscription()) {
        await this.loadInvoices();
      }
    } catch (err) {
      console.error('Erro ao carregar planos', err);
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  async loadInvoices() {
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      const response = await api.get(`/subscriptions/invoices?barbershopId=${barbershopId}`);
      this.invoices = response.data.data || [];
    } catch (err) {
      console.error('Erro ao carregar faturas', err);
    }
  }

  openCancelModal() {
    this.isCancelModalOpen = true;
  }

  closeCancelModal() {
    this.isCancelModalOpen = false;
  }

  async confirmCancelSubscription() {
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      await api.delete(`/subscriptions/cancel?barbershopId=${barbershopId}`);
      this.toastr.success('Assinatura cancelada com sucesso.', 'Cancelado');
      this.auth.setSubscriptionStatus('Bloqueada');
      this.closeCancelModal();
      this.router.navigate(['/']);
    } catch (err) {
      console.error('Erro ao cancelar', err);
      this.toastr.error('Erro ao cancelar assinatura. Tente novamente.', 'Erro');
    }
  }

  selectPlan(plan: any) {
    this.selectedPlan = plan;
  }

  cancelCheckout() {
    this.selectedPlan = null;
  }

  async processPayment() {
    if (!this.selectedPlan) return;
    
    if (!this.paymentData.holderName || !this.paymentData.number || !this.paymentData.expiryMonth || !this.paymentData.expiryYear || !this.paymentData.ccv) {
      this.toastr.warning('Por favor, preencha todos os campos do cartão de crédito.', 'Campos Obrigatórios');
      return;
    }

    this.processing = true;
    this.cdr.detectChanges();
    
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      
      // Clean credit card number format
      const ccNumber = this.paymentData.number.replace(/\s+/g, '');

      const payload = {
        planId: this.selectedPlan.id,
        creditCard: {
          holderName: this.paymentData.holderName,
          number: ccNumber,
          expiryMonth: this.paymentData.expiryMonth,
          expiryYear: this.paymentData.expiryYear,
          ccv: this.paymentData.ccv
        }
      };

      await api.post(`/subscriptions/checkout?barbershopId=${barbershopId}`, payload);
      this.toastr.success('Pagamento aprovado e assinatura ativada com sucesso!', 'Sucesso');
      this.auth.setSubscriptionStatus('Ativa');
      this.currentPlanId = this.selectedPlan.id;
      this.selectedPlan = null;
      setTimeout(() => this.router.navigate(['/dashboard']), 1500);
    } catch (err: any) {
      console.error('Erro ao processar pagamento', err);
      const msg = err.response?.data?.message || 'Falha ao processar o pagamento. Verifique os dados do cartão.';
      this.toastr.error(msg, 'Pagamento Recusado');
    } finally {
      this.processing = false;
      this.cdr.detectChanges();
    }
  }
  translateStatus(status: string): string {
    const map: Record<string, string> = {
      'PENDING': 'Pendente',
      'RECEIVED': 'Recebido',
      'CONFIRMED': 'Confirmado',
      'OVERDUE': 'Atrasado',
      'REFUNDED': 'Estornado',
      'RECEIVED_IN_CASH': 'Recebido em Dinheiro',
      'CHARGEBACK_REQUESTED': 'Chargeback'
    };
    return map[status] || status;
  }

  translateBillingType(type: string): string {
    const map: Record<string, string> = {
      'BOLETO': 'Boleto',
      'CREDIT_CARD': 'Cartão de Crédito',
      'PIX': 'Pix'
    };
    return map[type] || type;
  }
}
