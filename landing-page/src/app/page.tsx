import Image from "next/image";

export default function Home() {
  const whatsappNumber = "5511965079106";
  const whatsappMessage = encodeURIComponent("Olá! Quero conhecer o Saas de barbeiro");
  const whatsappUrl = `https://api.whatsapp.com/send/?phone=${whatsappNumber}&text=${whatsappMessage}&type=phone_number&app_absent=0`;

  return (
    <div className="container">
      <header className="header">
        <div className="logo">
          <Image src="/logo.png" alt="SaaS Barbearia Logo" width={30} height={30} style={{ objectFit: 'contain' }} />
        </div>
        <nav className="nav">
          <a href="#how-it-works">Como Funciona</a>
          <a href="#features">Benefícios</a>
          <a href="#pricing">Planos</a>
          <a href="#faq">FAQ</a>
          <a href={whatsappUrl} className="btn-outline" target="_blank" rel="noopener noreferrer">Falar Conosco</a>
        </nav>
      </header>

      <main>
        {/* 1. Hero */}
        <section className="hero">
          <div className="hero-content">
            <h1 className="hero-title">A evolução na gestão da sua <span>Barbearia</span></h1>
            <p className="hero-subtitle">
              Sua barbearia organizada em minutos. Agendamentos inteligentes, controle financeiro e assinaturas em um único lugar.
            </p>
            <div className="hero-actions">
              <a href={whatsappUrl} className="btn-primary" target="_blank" rel="noopener noreferrer">
                Criar conta grátis
              </a>
            </div>
          </div>
        </section>

        {/* 6. Screenshots (Placed here for immediate visual impact) */}
        <section className="screenshots">
          <div className="screenshot-wrapper">
             <Image 
               src="/dashboard-preview.png" 
               alt="Preview do Painel Web" 
               width={1000} 
               height={562} 
               style={{ width: '100%', height: 'auto', objectFit: 'cover' }} 
             />
          </div>
        </section>

        {/* 2. Como Funciona */}
        <section id="how-it-works" className="how-it-works section">
          <div className="section-header">
            <h2>Como Funciona</h2>
            <p>Em três passos simples você digitaliza todo o seu negócio.</p>
          </div>
          <div className="steps-grid">
            <div className="step-card">
              <div className="step-number">1</div>
              <h3>Crie sua conta</h3>
              <p>Cadastre-se rapidamente e configure o perfil da sua barbearia em poucos minutos.</p>
            </div>
            <div className="step-card">
              <div className="step-number">2</div>
              <h3>Cadastre sua equipe</h3>
              <p>Adicione seus barbeiros, horários de trabalho, escalas e todos os serviços que oferecem.</p>
            </div>
            <div className="step-card">
              <div className="step-number">3</div>
              <h3>Receba agendamentos</h3>
              <p>Seus clientes baixam o app e marcam horários direto pelo celular, sem conflitos.</p>
            </div>
          </div>
        </section>

        {/* 4. Comparativo */}
        <section className="comparison section">
          <div className="section-header">
            <h2>Por que mudar?</h2>
            <p>Esqueça os velhos problemas e venha para o futuro.</p>
          </div>
          <div className="comparison-grid">
            <div className="compare-card before">
              <div className="compare-header">Antes (Papel e WhatsApp)</div>
              <ul>
                <li>❌ Horários conflitantes e confusão</li>
                <li>❌ Clientes esquecem e faltam</li>
                <li>❌ Dificuldade para calcular comissões</li>
                <li>❌ Zero previsibilidade financeira</li>
              </ul>
            </div>
            <div className="compare-card after">
              <div className="compare-header">Depois (SaaS Barbearia)</div>
              <ul>
                <li>✅ Agenda 100% digital e sincronizada</li>
                <li>✅ Lembretes automáticos para clientes</li>
                <li>✅ Relatórios e comissões automáticas</li>
                <li>✅ Assinaturas gerando receita recorrente</li>
              </ul>
            </div>
          </div>
        </section>

        {/* 3. Benefícios */}
        <section id="features" className="features section">
          <div className="section-header">
            <h2>Benefícios</h2>
            <p>Um sistema completo, feito de barbeiro para barbeiro.</p>
          </div>
          <div className="features-grid">
            <div className="feature-card">
              <div className="feature-icon">📅</div>
              <h3>Agenda sem Conflito</h3>
              <p>Organize seus horários de forma automática, envie lembretes e reduza faltas com nossa agenda conectada.</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">✂️</div>
              <h3>Gestão de Equipe</h3>
              <p>Controle comissões, horários de trabalho e relatórios de desempenho individual de cada barbeiro.</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">🔔</div>
              <h3>Notificações</h3>
              <p>Avisos automáticos de marcação, cancelamento e conclusão direto no celular do cliente e do barbeiro.</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">📊</div>
              <h3>Faturamento</h3>
              <p>Visualize seu caixa, lucros e despesas em tempo real com gráficos interativos e precisos.</p>
            </div>
          </div>
        </section>

        {/* 5. Planos e Preços */}
        <section id="pricing" className="pricing section">
          <div className="section-header">
            <h2>Planos e Preços</h2>
            <p>Invista no crescimento da sua barbearia.</p>
          </div>
          <div className="pricing-card">
            <h3 className="plan-name">Plano Pro</h3>
            <div className="plan-price">R$ 39,99<span>/mês</span></div>
            <ul className="plan-features">
              <li>✔️ Agendamentos ilimitados</li>
              <li>✔️ Gestão ilimitada de barbeiros</li>
              <li>✔️ App exclusivo para clientes</li>
              <li>✔️ Integração com Asaas (Cobranças)</li>
              <li>✔️ Dashboard financeiro completo</li>
              <li>✔️ Controle de serviços e comissões</li>
              <li>✔️ Gestão de folgas e escalas</li>
              <li>✔️ Lembretes automáticos (Notificações)</li>
              <li>✔️ Suporte especializado</li>
            </ul>
            <a href={whatsappUrl} className="btn-primary full-width" target="_blank" rel="noopener noreferrer">
              Assinar Agora
            </a>
          </div>
        </section>

        {/* 7. FAQ */}
        <section id="faq" className="faq section">
          <div className="section-header">
            <h2>Perguntas Frequentes</h2>
          </div>
          <div className="faq-list">
            <div className="faq-item">
              <h4>Como funciona a cobrança?</h4>
              <p>A cobrança é mensal e automática no seu cartão de crédito, processada com segurança através da nossa parceria com o Asaas.</p>
            </div>
            <div className="faq-item">
              <h4>Tem período de teste?</h4>
              <p>Sim! Entre em contato conosco no WhatsApp para liberar um acesso de degustação exclusivo para você.</p>
            </div>
            <div className="faq-item">
              <h4>Como funciona o cancelamento?</h4>
              <p>Você pode cancelar a qualquer momento no seu painel. Seu acesso continua ativo até o final do período que já foi pago, sem multas.</p>
            </div>
            <div className="faq-item">
              <h4>Tenho suporte se precisar de ajuda?</h4>
              <p>Com certeza. Nosso suporte atende via WhatsApp de segunda a sexta, em horário comercial, para tirar todas as suas dúvidas.</p>
            </div>
          </div>
        </section>

        <section className="cta-bottom">
          <h2>Pronto para lotar sua agenda?</h2>
          <p>Fale conosco e comece agora mesmo.</p>
          <a href={whatsappUrl} className="btn-primary" target="_blank" rel="noopener noreferrer">
            Criar conta grátis
          </a>
        </section>

      </main>

      {/* 8. Rodapé */}
      <footer className="footer">
        <div className="footer-content">
          <div className="logo">
            <Image src="/logo.png" alt="SaaS Barbearia Logo" width={24} height={24} style={{ objectFit: 'contain' }} />
            SaaS Barbearia
          </div>
          <div className="footer-links">
            <a href="#">Política de Privacidade</a>
            <a href="#">Termos de Uso</a>
            <a href={whatsappUrl} target="_blank" rel="noopener noreferrer">Contato</a>
          </div>
          <p className="copyright">© 2026 Desenvolvido por Caio Santos. Todos os direitos reservados.</p>
        </div>
      </footer>
    </div>
  );
}
