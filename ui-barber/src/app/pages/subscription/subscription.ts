import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { api } from '../../services/api';
import { ToastrService } from 'ngx-toastr';
import { NgxMaskDirective } from 'ngx-mask';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';

import { GlobalService } from '../../services/global.service';

@Component({
  selector: 'app-subscription',
  standalone: true,
  imports: [CommonModule, FormsModule, NgxMaskDirective],
  templateUrl: './subscription.html',
  styleUrls: ['./subscription.css']
})
export class Subscription extends GlobalService implements OnInit {
  plans: any[] = [];
  loading = true;
  processing = false;
  canceling = false;
  isCancelModalOpen = false;
  currentPlanId = '';
  selectedPlan: any = null;
  billingType: 'CREDIT_CARD' | 'PIX' | 'BOLETO' = 'CREDIT_CARD';
  invoices: any[] = [];

  pixResult: { pixQrCode?: string; pixKey?: string; expirationDate?: string } | null = null;
  boletoResult: { boletoUrl?: string; boletoBarCode?: string; dueDate?: string } | null = null;

  paymentData = {
    holderName: '',
    number: '',
    expiryMonth: '',
    expiryYear: '',
    ccv: ''
  };

  constructor(
    private cdr: ChangeDetectorRef,
    public auth: Auth
  ) {
    super();
  }

  ngOnInit() {
    this.loadPlans();
  }

  async loadPlans() {
    this.spinnerShow();
    this.loading = true;
    this.cdr.detectChanges();
    try {
      const response = await api.get(`/plans?deleted=false`);
      this.plans = response.data.data || [];

      const barbershopId = localStorage.getItem('barbershopId');
      const shopRes = await api.get(`/barbershops?deleted=false`);
      if (shopRes.data.data && shopRes.data.data.length > 0) {
        const myShop = shopRes.data.data.find((s: any) => s.id === barbershopId) || shopRes.data.data[0];
        this.currentPlanId = myShop.planId;
        if (myShop.subscriptionStatus) {
          this.auth.setSubscriptionStatus(myShop.subscriptionStatus);
        }
      }

      await this.loadInvoices();
    } catch (err) {
      this.errorNotification(err);
      console.error('Erro ao carregar planos', err);
    } finally {
      this.loading = false;
      this.spinnerHide();
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

  get pendingInvoices(): any[] {
    return this.invoices.filter(i => i.status === 'PENDING' || i.status === 'OVERDUE');
  }

  selectPlan(plan: any) {
    this.selectedPlan = plan;
    this.billingType = 'CREDIT_CARD';
    this.pixResult = null;
    this.boletoResult = null;
    this.paymentData = { holderName: '', number: '', expiryMonth: '', expiryYear: '', ccv: '' };
  }

  cancelCheckout() {
    this.selectedPlan = null;
    this.pixResult = null;
    this.boletoResult = null;
  }

  selectBillingType(type: 'CREDIT_CARD' | 'PIX' | 'BOLETO') {
    this.billingType = type;
    this.pixResult = null;
    this.boletoResult = null;
  }

  openCancelModal() { this.isCancelModalOpen = true; }
  closeCancelModal() { this.isCancelModalOpen = false; }

  async confirmCancelSubscription() {
    if (this.canceling) return;
    this.canceling = true;
    this.cdr.detectChanges();
    try {
      const barbershopId = localStorage.getItem('barbershopId');
      await api.delete(`/subscriptions/cancel?barbershopId=${barbershopId}`);
      this.toastr.success('Assinatura cancelada com sucesso.', 'Cancelado');
      this.auth.setSubscriptionStatus('Bloqueada');
      this.closeCancelModal();
      this.router.navigate(['/']);
    } catch (err) {
      this.toastr.error('Erro ao cancelar assinatura. Tente novamente.', 'Erro');
    } finally {
      this.canceling = false;
      this.cdr.detectChanges();
    }
  }

  async processPayment() {
    if (this.processing || !this.selectedPlan) return;

    if (this.billingType === 'CREDIT_CARD') {
      if (!this.paymentData.holderName || !this.paymentData.number || !this.paymentData.expiryMonth || !this.paymentData.expiryYear || !this.paymentData.ccv) {
        this.toastr.warning('Por favor, preencha todos os campos do cartão de crédito.', 'Campos Obrigatórios');
        return;
      }
    }

    this.processing = true;
    this.cdr.detectChanges();

    try {
      const barbershopId = localStorage.getItem('barbershopId');
      const ccNumber = this.paymentData.number.replace(/\s+/g, '');

      const payload: any = {
        planId: this.selectedPlan.id,
        billingType: this.billingType
      };

      if (this.billingType === 'CREDIT_CARD') {
        payload.creditCard = {
          holderName: this.paymentData.holderName,
          number: ccNumber,
          expiryMonth: this.paymentData.expiryMonth,
          expiryYear: this.paymentData.expiryYear,
          ccv: this.paymentData.ccv
        };
      }

      const res = await api.post(`/subscriptions/checkout?barbershopId=${barbershopId}`, payload);
      const data = res.data?.data || res.data;

      this.currentPlanId = this.selectedPlan.id;

      if (this.billingType === 'PIX' && data?.pixKey) {
        this.auth.setSubscriptionStatus('Bloqueada');
        this.pixResult = data;
        this.toastr.success('Assinatura gerada! Pague via PIX para ativar.', 'PIX Gerado');
        await this.loadInvoices();
      } else if (this.billingType === 'BOLETO' && data?.boletoUrl) {
        this.auth.setSubscriptionStatus('Bloqueada');
        this.boletoResult = data;
        this.toastr.success('Boleto gerado! Pague para ativar sua assinatura.', 'Boleto Gerado');
        await this.loadInvoices();
      } else {
        this.auth.setSubscriptionStatus('Ativa');
        this.toastr.success('Pagamento aprovado e assinatura ativada com sucesso!', 'Sucesso');
        this.selectedPlan = null;
        setTimeout(() => this.router.navigate(['/dashboard']), 1500);
      }
    } catch (err: any) {
      console.error('Erro ao processar pagamento', err);
      const msg = err.response?.data?.message || 'Falha ao processar o pagamento. Verifique os dados e tente novamente.';
      this.toastr.error(msg, 'Pagamento Recusado');
    } finally {
      this.processing = false;
      this.cdr.detectChanges();
    }
  }

  copyPixKey() {
    if (this.pixResult?.pixKey) {
      navigator.clipboard.writeText(this.pixResult.pixKey);
      this.toastr.info('Chave PIX copiada!', 'Copiado');
    }
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  isPayModalOpen = false;
  selectedInvoice: any = null;
  invoiceBillingType: 'PIX' | 'BOLETO' | 'CREDIT_CARD' = 'PIX';
  invoicePixLoading = false;
  invoicePixData: { pixQrCode?: string; pixKey?: string; expirationDate?: string } | null = null;
  invoiceCardData = {
    holderName: '',
    number: '',
    expiryMonth: '',
    expiryYear: '',
    ccv: ''
  };
  payingInvoice = false;

  openPayModal(invoice: any) {
    this.selectedInvoice = invoice;
    this.isPayModalOpen = true;
    this.invoiceBillingType = invoice.billingType === 'BOLETO' ? 'BOLETO' : invoice.billingType === 'CREDIT_CARD' ? 'CREDIT_CARD' : 'PIX';
    this.invoicePixData = null;
    this.invoiceCardData = { holderName: '', number: '', expiryMonth: '', expiryYear: '', ccv: '' };
    if (this.invoiceBillingType === 'PIX') {
      this.loadInvoicePix(invoice.id);
    }
  }

  closePayModal() {
    this.isPayModalOpen = false;
    this.selectedInvoice = null;
    this.invoicePixData = null;
  }

  selectInvoiceBillingType(type: 'PIX' | 'BOLETO' | 'CREDIT_CARD') {
    this.invoiceBillingType = type;
    if (type === 'PIX' && !this.invoicePixData && this.selectedInvoice) {
      this.loadInvoicePix(this.selectedInvoice.id);
    }
  }

  async loadInvoicePix(paymentId: string) {
    this.invoicePixLoading = true;
    this.cdr.detectChanges();
    try {
      const res = await api.get(`/subscriptions/invoices/${paymentId}/pix`);
      this.invoicePixData = res.data?.data || res.data;
    } catch (err) {
      this.toastr.error('Não foi possível carregar a chave PIX desta fatura.', 'Erro');
    } finally {
      this.invoicePixLoading = false;
      this.cdr.detectChanges();
    }
  }

  copyInvoicePixKey() {
    if (this.invoicePixData?.pixKey) {
      navigator.clipboard.writeText(this.invoicePixData.pixKey);
      this.toastr.info('Chave PIX copiada!', 'Copiado');
    }
  }

  async payInvoiceWithCard() {
    if (this.payingInvoice || !this.selectedInvoice) return;
    if (!this.invoiceCardData.holderName || !this.invoiceCardData.number || !this.invoiceCardData.expiryMonth || !this.invoiceCardData.expiryYear || !this.invoiceCardData.ccv) {
      this.toastr.warning('Preencha todos os campos do cartão.', 'Campos Obrigatórios');
      return;
    }

    this.payingInvoice = true;
    this.cdr.detectChanges();
    try {
      const ccNumber = this.invoiceCardData.number.replace(/\s+/g, '');
      await api.post(`/subscriptions/invoices/${this.selectedInvoice.id}/pay-card`, {
        holderName: this.invoiceCardData.holderName,
        number: ccNumber,
        expiryMonth: this.invoiceCardData.expiryMonth,
        expiryYear: this.invoiceCardData.expiryYear,
        ccv: this.invoiceCardData.ccv
      });
      this.toastr.success('Pagamento realizado com sucesso!', 'Fatura Paga');
      this.closePayModal();
      await this.loadInvoices();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Erro ao processar pagamento do cartão.';
      this.toastr.error(msg, 'Erro no Pagamento');
    } finally {
      this.payingInvoice = false;
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

  getStatusClass(status: string): string {
    const s = (status || '').toUpperCase();
    if (['RECEIVED', 'CONFIRMED', 'RECEIVED_IN_CASH'].includes(s)) {
      return 'active';
    }
    if (['PENDING'].includes(s)) {
      return 'pending';
    }
    if (['OVERDUE', 'REFUNDED', 'CHARGEBACK_REQUESTED', 'CANCELLED'].includes(s)) {
      return 'inactive';
    }
    return 'pending';
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
