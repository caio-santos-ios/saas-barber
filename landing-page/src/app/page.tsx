import Image from "next/image";
import TrackedLink from "./TrackedLink";

const whatsappNumber = "5511965079106";
const adminBaseUrl = (process.env.NEXT_PUBLIC_ADMIN_URL || "https://saas-barber-k7nn.vercel.app").replace(/\/$/, "");
const signupUrl = adminBaseUrl ? `${adminBaseUrl}/register` : "#contact";
const signupLabel = adminBaseUrl ? "Criar minha conta grátis" : "Quero começar com ajuda";

function whatsappUrl(source: string) {
  const message = encodeURIComponent(
    `Olá! Sou dono de uma barbearia e quero conhecer o SaaS Barbearia. Origem: ${source}.`,
  );
  return `https://api.whatsapp.com/send/?phone=${whatsappNumber}&text=${message}&type=phone_number&app_absent=0`;
}

const productJsonLd = {
  "@context": "https://schema.org",
  "@type": "SoftwareApplication",
  name: "SaaS Barbearia",
  applicationCategory: "BusinessApplication",
  operatingSystem: "Web, Android, iOS",
  description:
    "Sistema de gestão para barbearias com painel administrativo, agenda, controle financeiro, comissões e aplicativos para barbeiros e clientes.",
  offers: [
    {
      "@type": "Offer",
      name: "Plano Barbearia Fundadora",
      price: "29.90",
      priceCurrency: "BRL",
      url: "https://saas-barber-xi.vercel.app/#pricing",
      description: "Condição especial para as 10 primeiras barbearias durante 6 meses.",
      availability: "https://schema.org/InStock",
    },
    {
      "@type": "Offer",
      name: "Plano Pro",
      price: "39.99",
      priceCurrency: "BRL",
      url: "https://saas-barber-xi.vercel.app/#pricing",
      description: "Preço oficial mensal do plano Pro.",
      availability: "https://schema.org/InStock",
    },
  ],
};

const benefits = [
  {
    number: "01",
    title: "Agenda organizada",
    description:
      "Veja os horários de toda a equipe em um único lugar e reduza conflitos na operação.",
  },
  {
    number: "02",
    title: "Comissões sem discussão",
    description:
      "Registre os atendimentos e acompanhe o que cada profissional produziu no período.",
  },
  {
    number: "03",
    title: "Caixa sob controle",
    description:
      "Acompanhe faturamento, serviços e despesas para tomar decisões com mais segurança.",
  },
  {
    number: "04",
    title: "Cliente mais independente",
    description:
      "O cliente escolhe barbeiro, serviço, data e horário pelo próprio aplicativo.",
  },
];

const faqs = [
  {
    question: "Como começo a usar o SaaS Barbearia?",
    answer:
      "Crie a conta do administrador, configure sua barbearia, cadastre os serviços e adicione os profissionais da equipe. Depois, você já pode organizar a agenda e convidar seus clientes para usar o aplicativo.",
  },
  {
    question: "Quais produtos estão incluídos?",
    answer:
      "O ecossistema reúne o painel Web do administrador, o aplicativo do barbeiro e o aplicativo do cliente. Cada perfil acessa apenas as funções necessárias para sua rotina.",
  },
  {
    question: "O cliente consegue escolher o barbeiro e o serviço?",
    answer:
      "Sim. No aplicativo do cliente, ele escolhe o barbeiro, o serviço, a data e um horário disponível antes de confirmar o agendamento.",
  },
  {
    question: "Como funciona a cobrança?",
    answer:
      "A condição de lançamento para as 10 primeiras barbearias é de R$ 29,90 por mês durante os 6 primeiros meses. Depois desse período, aplica-se o preço oficial de R$ 39,99 por mês, com cobrança recorrente processada pelo Asaas.",
  },
  {
    question: "Preciso instalar algo para começar?",
    answer:
      "O dono da barbearia começa pelo painel Web. Os barbeiros usam o aplicativo da equipe e os clientes usam o aplicativo de agendamento, cada um com as funções do seu perfil.",
  },
  {
    question: "Tenho ajuda para configurar a barbearia?",
    answer:
      "Sim. Se você falar com a equipe pelo WhatsApp, podemos orientar a configuração inicial de serviços, profissionais, horários e divulgação do aplicativo para os clientes.",
  },
];

function Brand() {
  return (
    <a href="#top" className="brand" aria-label="SaaS Barbearia — início">
      <Image src="/logo.png" alt="" width={36} height={36} className="brand-mark" />
      <span>
        SaaS <strong>Barbearia</strong>
      </span>
    </a>
  );
}

function ClientAppPreview() {
  return (
    <div className="phone-frame" aria-label="Prévia ilustrativa do app do cliente">
      <div className="phone-notch" />
      <div className="phone-content client-preview">
        <div className="phone-topline">
          <span>09:41</span>
          <span className="phone-signal">•••</span>
        </div>
        <div className="app-brand-line">
          <span className="mini-logo">B</span>
          <span>Barbearia Premium</span>
          <span className="bell">○</span>
        </div>
        <p className="phone-kicker">Olá, João</p>
        <h4>Agende seu próximo corte</h4>
        <div className="booking-highlight">
          <span className="calendar-icon">24</span>
          <span>
            <strong>Corte + Barba</strong>
            <small>Escolha um horário disponível</small>
          </span>
          <span className="arrow">→</span>
        </div>
        <p className="phone-section-label">Próximo agendamento</p>
        <div className="appointment-card">
          <div>
            <strong>Amanhã, 14:00</strong>
            <span>Com Caio Santos</span>
          </div>
          <span className="status-pill">Marcado</span>
        </div>
        <div className="phone-nav">
          <span className="active">Início</span>
          <span>Agenda</span>
          <span>Perfil</span>
        </div>
      </div>
    </div>
  );
}

function BarberAppPreview() {
  return (
    <div className="phone-frame" aria-label="Prévia ilustrativa do app do barbeiro">
      <div className="phone-notch" />
      <div className="phone-content barber-preview">
        <div className="phone-topline">
          <span>09:41</span>
          <span className="phone-signal">•••</span>
        </div>
        <div className="app-brand-line">
          <span className="mini-logo">B</span>
          <span>Minha agenda</span>
          <span className="bell">○</span>
        </div>
        <p className="phone-kicker">Quarta, 20 de agosto</p>
        <h4>Seus atendimentos</h4>
        <div className="barber-stat-row">
          <div>
            <strong>06</strong>
            <span>agendados</span>
          </div>
          <div>
            <strong>R$ 420</strong>
            <span>no período</span>
          </div>
        </div>
        <p className="phone-section-label">Próximos horários</p>
        <div className="barber-appointment active-appointment">
          <span className="time-mark">09:00</span>
          <span>
            <strong>João Silva</strong>
            <small>Corte + Barba · 45 min</small>
          </span>
          <span className="dot-status" />
        </div>
        <div className="barber-appointment">
          <span className="time-mark">10:30</span>
          <span>
            <strong>Marcos Lima</strong>
            <small>Corte · 30 min</small>
          </span>
          <span className="dot-status muted" />
        </div>
        <div className="phone-nav">
          <span className="active">Agenda</span>
          <span>Serviços</span>
          <span>Perfil</span>
        </div>
      </div>
    </div>
  );
}

export default function Home() {
  const primaryDestination = adminBaseUrl ? signupUrl : whatsappUrl("hero");
  const pricingDestination = adminBaseUrl ? signupUrl : whatsappUrl("planos");
  const finalDestination = adminBaseUrl ? signupUrl : whatsappUrl("cta-final");

  return (
    <div className="site-shell" id="top">
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(productJsonLd) }}
      />

      <header className="header">
        <div className="header-inner">
          <Brand />
          <nav className="nav" aria-label="Navegação principal">
            <a href="#produto">Produtos</a>
            <a href="#beneficios">Benefícios</a>
            <a href="#pricing">Planos</a>
            <a href="#faq">FAQ</a>
            <TrackedLink
              href={primaryDestination}
              className="btn-outline"
              target={adminBaseUrl ? undefined : "_blank"}
              rel={adminBaseUrl ? undefined : "noopener noreferrer"}
              eventName="cta_click"
              eventParams={{ cta_location: "nav", cta_destination: adminBaseUrl ? "signup" : "whatsapp" }}
            >
              {adminBaseUrl ? "Começar agora" : "Falar conosco"}
            </TrackedLink>
          </nav>
          <TrackedLink
            href={primaryDestination}
            className="mobile-cta"
            target={adminBaseUrl ? undefined : "_blank"}
            rel={adminBaseUrl ? undefined : "noopener noreferrer"}
            eventName="cta_click"
            eventParams={{ cta_location: "mobile-nav", cta_destination: adminBaseUrl ? "signup" : "whatsapp" }}
          >
            Começar
          </TrackedLink>
        </div>
      </header>

      <main>
        <section className="hero section-shell">
          <div className="hero-copy">
            <div className="eyebrow"><span className="eyebrow-dot" /> Gestão para barbearias que querem crescer</div>
            <h1>Organize agenda, equipe e caixa da sua barbearia em um só lugar.</h1>
            <p className="hero-lead">
              Um sistema completo para o dono administrar a operação, o barbeiro acompanhar a própria rotina e o cliente agendar sem depender de várias mensagens.
            </p>
            <div className="hero-actions">
              <TrackedLink
                href={primaryDestination}
                className="btn-primary btn-large"
                target={adminBaseUrl ? undefined : "_blank"}
                rel={adminBaseUrl ? undefined : "noopener noreferrer"}
                eventName="cta_click"
                eventParams={{ cta_location: "hero", cta_destination: adminBaseUrl ? "signup" : "whatsapp" }}
              >
                {signupLabel} <span aria-hidden="true">→</span>
              </TrackedLink>
              <a href="#produto" className="text-link">Ver como funciona <span aria-hidden="true">↓</span></a>
            </div>
            <p className="hero-assurance">Cadastro rápido · Configuração acompanhada · Cancele quando quiser</p>
            <div className="hero-proof" aria-label="Benefícios de começar">
              <span><i>✓</i> Configuração simples</span>
              <span><i>✓</i> Suporte em português</span>
              <span><i>✓</i> Cancele quando quiser</span>
            </div>
          </div>

          <div className="hero-visual">
            <div className="visual-glow" />
            <div className="browser-window">
              <div className="browser-bar">
                <div className="browser-dots"><span /><span /><span /></div>
                <div className="browser-address">app.saasbarbearia.com.br/dashboard</div>
                <span className="browser-lock">●</span>
              </div>
              <div className="dashboard-label"><span className="live-dot" /> Painel do administrador</div>
              <Image
                src="/dashboard-preview.png"
                alt="Dashboard do SaaS Barbearia com faturamento, agendamentos e agenda do período"
                width={1024}
                height={498}
                priority
                className="dashboard-image"
              />
            </div>
            <div className="floating-note floating-note-one"><span className="note-icon">↗</span><span><strong>Agenda em ordem</strong><small>Veja sua equipe de uma só vez</small></span></div>
            <div className="floating-note floating-note-two"><span className="note-icon gold">✓</span><span><strong>Feito para a rotina</strong><small>Admin, barbeiro e cliente</small></span></div>
          </div>
        </section>

        <section className="problem-strip">
          <div className="section-shell problem-inner">
            <p className="section-eyebrow">Se você se reconhece aqui, o SaaS foi feito para você</p>
            <div className="problem-list">
              <span>Agenda no WhatsApp</span><span>Faltas sem aviso</span><span>Comissão na calculadora</span><span>Caixa difícil de acompanhar</span>
            </div>
          </div>
        </section>

        <section id="produto" className="section-shell product-section">
          <div className="section-heading split-heading">
            <div>
              <p className="section-eyebrow">Um ecossistema, três experiências</p>
              <h2>Cada pessoa cuida do que importa para ela.</h2>
            </div>
            <p>O administrador enxerga o negócio. O barbeiro acompanha a própria rotina. O cliente agenda sem depender de mensagens.</p>
          </div>

          <div className="product-grid">
            <article className="product-card admin-card">
              <div className="product-card-copy">
                <span className="product-index">01 · PAINEL WEB</span>
                <h3>Controle a operação sem abrir cinco planilhas.</h3>
                <p>Um painel para o dono administrar a barbearia, acompanhar o caixa e manter a equipe alinhada.</p>
                <div className="tag-row"><span>Dashboard</span><span>Agenda</span><span>Financeiro</span><span>Equipe</span></div>
              </div>
              <div className="admin-screen">
                <div className="admin-screen-sidebar"><span className="mini-logo">B</span><span className="sidebar-active" /><span /><span /><span /><span /></div>
                <div className="admin-screen-main">
                  <div className="screen-topline"><span>Visão geral</span><span className="screen-date">15 ago — 14 set</span></div>
                  <div className="metric-row"><div><small>Faturamento</small><strong>R$ 4.850</strong></div><div><small>Agendamentos</small><strong>128</strong></div><div><small>Finalizados</small><strong className="green">112</strong></div></div>
                  <div className="screen-table"><div className="table-line wide" /><div className="table-line" /><div className="table-line short" /><div className="table-line wide" /></div>
                </div>
              </div>
            </article>

            <article className="product-card app-card client-card">
              <div className="product-card-copy">
                <span className="product-index">02 · APP DO CLIENTE</span>
                <h3>Agendamento simples para quem vai cortar.</h3>
                <p>Seu cliente escolhe serviço, barbeiro, data e horário pelo aplicativo e acompanha os próprios agendamentos.</p>
                <div className="tag-row"><span>Home</span><span>Agendamentos</span><span>Perfil</span></div>
              </div>
              <ClientAppPreview />
            </article>

            <article className="product-card app-card barber-card">
              <div className="product-card-copy">
                <span className="product-index">03 · APP DO BARBEIRO</span>
                <h3>Mais clareza para atender bem.</h3>
                <p>O barbeiro visualiza a agenda do dia, atualiza o status do atendimento e acompanha seus dados.</p>
                <div className="tag-row"><span>Minha agenda</span><span>Serviços</span><span>Financeiro</span></div>
              </div>
              <BarberAppPreview />
            </article>
          </div>
        </section>

        <section className="section-shell screens-section" aria-labelledby="screens-title">
          <div className="section-heading centered-heading">
            <p className="section-eyebrow">Veja o painel por dentro</p>
            <h2 id="screens-title">Mais clareza para cuidar da operação.</h2>
            <p>Uma visão real das ferramentas que ajudam a barbearia a organizar agenda, clientes e equipe em um só lugar.</p>
          </div>
          <div className="screens-grid">
            <figure className="screen-shot screen-shot-feature">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-1.png.png"
                  alt="Dashboard real do painel Web do SaaS Barbearia"
                  width={1573}
                  height={890}
                  sizes="(max-width: 760px) 100vw, 72vw"
                />
              </div>
              <figcaption><strong>Dashboard</strong><span>Uma visão rápida do faturamento, agendamentos e desempenho da barbearia.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-2.png.png"
                  alt="Tela de agenda do painel Web do SaaS Barbearia"
                  width={1587}
                  height={890}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Agenda</strong><span>Horários, clientes, profissionais e status em uma única tela.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-3.png.png"
                  alt="Tela de clientes do painel Web do SaaS Barbearia"
                  width={1585}
                  height={887}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Clientes</strong><span>Cadastro, contatos, status e acesso rápido às ações da equipe.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-4.png.png"
                  alt="Tela de profissionais do painel Web do SaaS Barbearia"
                  width={1588}
                  height={889}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Profissionais</strong><span>Equipe, contatos, status e gestão dos barbeiros da barbearia.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-5.png.png"
                  alt="Tela de serviços do painel Web do SaaS Barbearia"
                  width={1570}
                  height={884}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Serviços</strong><span>Catálogo de serviços, duração, preços e ações para manter a oferta organizada.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-6.png.png"
                  alt="Tela de escalas de trabalho do painel Web do SaaS Barbearia"
                  width={1584}
                  height={887}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Escalas de trabalho</strong><span>Horários e disponibilidade da equipe organizados por dia da semana.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-7.png.png"
                  alt="Tela de divulgação e código da barbearia no painel Web"
                  width={1567}
                  height={887}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Divulgação do app</strong><span>Recursos para compartilhar o acesso da barbearia e facilitar a chegada dos clientes.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-8.png.png"
                  alt="Tela de assinatura e planos do painel Web do SaaS Barbearia"
                  width={1588}
                  height={890}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Assinatura e planos</strong><span>Acompanhe o plano contratado, cobranças e situação da assinatura.</span></figcaption>
            </figure>
            <figure className="screen-shot">
              <div className="screen-shot-frame">
                <Image
                  src="/screens/projeto-14-9.png.png"
                  alt="Tela de configurações da barbearia no painel Web"
                  width={1592}
                  height={888}
                  sizes="(max-width: 760px) 100vw, 50vw"
                />
              </div>
              <figcaption><strong>Configurações</strong><span>Dados da barbearia, contatos e informações operacionais em um só lugar.</span></figcaption>
            </figure>
          </div>
        </section>

        <section className="section-shell app-showcase-section" aria-labelledby="barber-app-title">
          <div className="section-heading split-heading">
            <div>
              <p className="section-eyebrow">App do profissional</p>
              <h2 id="barber-app-title">A rotina do barbeiro na palma da mão.</h2>
            </div>
            <p>Da escolha da barbearia à agenda do dia, o profissional acompanha seus horários e atendimentos sem depender de planilhas ou mensagens soltas.</p>
          </div>
          <div className="barber-app-gallery">
            <figure className="barber-app-shot barber-app-shot-feature">
              <div className="barber-app-device"><Image src="/app-barbeiro/minha-agenda.jpg" alt="Tela Minha Agenda do app do barbeiro Na Régua" width={1080} height={2340} sizes="(max-width: 760px) 48vw, 220px" loading="lazy" /></div>
              <figcaption><strong>Minha agenda</strong><span>Visualize os horários do dia e os detalhes de cada atendimento.</span></figcaption>
            </figure>
            <figure className="barber-app-shot">
              <div className="barber-app-device"><Image src="/app-barbeiro/servicos.jpg" alt="Tela de serviços do app do barbeiro Na Régua" width={1080} height={2340} sizes="(max-width: 760px) 48vw, 220px" loading="lazy" /></div>
              <figcaption><strong>Serviços</strong><span>Consulte os serviços disponíveis e organize o atendimento.</span></figcaption>
            </figure>
            <figure className="barber-app-shot">
              <div className="barber-app-device"><Image src="/app-barbeiro/financeiro.jpg" alt="Tela de acompanhamento de resultados do app do barbeiro Na Régua" width={1080} height={2340} sizes="(max-width: 760px) 48vw, 220px" loading="lazy" /></div>
              <figcaption><strong>Resultados</strong><span>Acompanhe o resumo dos serviços concluídos.</span></figcaption>
            </figure>
            <figure className="barber-app-shot">
              <div className="barber-app-device"><Image src="/app-barbeiro/profile.jpg" alt="Tela de perfil profissional do app do barbeiro Na Régua" width={1080} height={2340} sizes="(max-width: 760px) 48vw, 220px" loading="lazy" /></div>
              <figcaption><strong>Perfil profissional</strong><span>Mantenha seus dados e preferências organizados.</span></figcaption>
            </figure>
            <figure className="barber-app-shot">
              <div className="barber-app-device"><Image src="/app-barbeiro/selecionar-barbearia.jpg" alt="Tela de seleção de barbearia do app do barbeiro Na Régua" width={1080} height={2340} sizes="(max-width: 760px) 48vw, 220px" loading="lazy" /></div>
              <figcaption><strong>Acesso à barbearia</strong><span>Entre na operação correta com poucos passos.</span></figcaption>
            </figure>
            <figure className="barber-app-shot">
              <div className="barber-app-device"><Image src="/app-barbeiro/selecionar-barbearia-codigo.jpg" alt="Tela de código da barbearia do app do barbeiro Na Régua" width={1080} height={2340} sizes="(max-width: 760px) 48vw, 220px" loading="lazy" /></div>
              <figcaption><strong>Código da barbearia</strong><span>Conecte-se à equipe usando o código disponibilizado.</span></figcaption>
            </figure>
            <figure className="barber-app-shot">
              <div className="barber-app-device"><Image src="/app-barbeiro/login.jpg" alt="Tela de login profissional do app do barbeiro Na Régua" width={1080} height={2340} sizes="(max-width: 760px) 48vw, 220px" loading="lazy" /></div>
              <figcaption><strong>Login profissional</strong><span>Acesse sua rotina com segurança pelo mesmo aplicativo.</span></figcaption>
            </figure>
          </div>
        </section>

        <section id="beneficios" className="section-shell benefits-section">
          <div className="section-heading centered-heading">
            <p className="section-eyebrow">Feito para a rotina real</p>
            <h2>Menos retrabalho. Mais controle para crescer.</h2>
            <p>Você não precisa de mais uma ferramenta complicada. Precisa saber o que está acontecendo na sua barbearia.</p>
          </div>
          <div className="benefits-grid">
            {benefits.map((benefit) => (
              <article className="benefit-card" key={benefit.number}>
                <span className="benefit-number">{benefit.number}</span>
                <h3>{benefit.title}</h3>
                <p>{benefit.description}</p>
              </article>
            ))}
          </div>
        </section>

        <section id="how-it-works" className="section-shell process-section">
          <div className="section-heading split-heading">
            <div>
              <p className="section-eyebrow">Comece sem complicação</p>
              <h2>Da conta criada à agenda funcionando.</h2>
            </div>
            <p>Você configura a operação uma vez e passa a trabalhar com uma visão mais organizada da barbearia.</p>
          </div>
          <div className="process-grid">
            <article className="process-step"><span>01</span><div><h3>Crie sua conta</h3><p>Cadastre a barbearia e o administrador em poucos minutos.</p></div></article>
            <article className="process-step"><span>02</span><div><h3>Configure a equipe</h3><p>Adicione serviços, profissionais, horários e regras da operação.</p></div></article>
            <article className="process-step"><span>03</span><div><h3>Compartilhe o app</h3><p>Convide barbeiros e clientes para deixar os agendamentos mais independentes.</p></div></article>
          </div>
          <p className="process-note"><strong>Entrou como barbearia fundadora?</strong> Você recebe ajuda para configurar profissionais, serviços e horários iniciais.</p>
        </section>

        <section className="section-shell comparison-section">
          <div className="comparison-card">
            <div className="comparison-column old-way"><span className="comparison-label">ANTES</span><h3>O negócio preso ao improviso.</h3><p>Horários espalhados, cliente esperando resposta e comissão conferida no fim do mês.</p><div className="comparison-line"><span>×</span> Agenda no papel e WhatsApp</div><div className="comparison-line"><span>×</span> Faltas sem lembrete</div><div className="comparison-line"><span>×</span> Números difíceis de enxergar</div></div>
            <div className="comparison-divider"><span>→</span></div>
            <div className="comparison-column new-way"><span className="comparison-label">DEPOIS</span><h3>Uma operação mais previsível.</h3><p>Agenda sincronizada, equipe alinhada e informações que ajudam você a decidir.</p><div className="comparison-line"><span>✓</span> Agendamentos organizados</div><div className="comparison-line"><span>✓</span> Gestão por profissional</div><div className="comparison-line"><span>✓</span> Visão de caixa e desempenho</div></div>
          </div>
        </section>

        <section className="section-shell trust-section">
          <div className="trust-card">
            <div className="trust-intro"><p className="section-eyebrow">Por que começar agora</p><h2>Você não precisa configurar tudo sozinho.</h2><p>A condição de fundador foi criada para as primeiras barbearias que querem testar a operação com acompanhamento próximo e ajudar a evoluir o produto.</p></div>
            <div className="trust-points"><div className="trust-point"><strong>01</strong><span><b>Teste com uma rotina real</b><small>Use profissionais, serviços e horários da sua própria barbearia.</small></span></div><div className="trust-point"><strong>02</strong><span><b>Tenha ajuda na configuração</b><small>Comece com orientação para colocar a agenda em funcionamento.</small></span></div><div className="trust-point"><strong>03</strong><span><b>Contribua com a próxima fase</b><small>Seu feedback ajuda a construir uma ferramenta melhor para o segmento.</small></span></div></div>
          </div>
        </section>

        <section id="pricing" className="section-shell pricing-section">
          <div className="pricing-copy">
            <p className="section-eyebrow">Condição especial de lançamento</p>
            <h2>Comece agora com uma condição especial de lançamento.</h2>
            <p>Use o sistema completo por uma condição especial durante os 6 primeiros meses e ajude a construir a próxima fase do produto.</p>
            <div className="pricing-notes"><span>✓ R$ 29,90 nos 6 primeiros meses</span><span>✓ Agendamentos e profissionais ilimitados</span><span>✓ Configuração acompanhada</span></div>
          </div>
          <div className="pricing-card">
            <div className="pricing-card-top"><span className="plan-badge">PLANO FUNDADOR</span><span className="plan-note">10 vagas</span></div>
            <div className="price">R$ 29<span>,90</span><small>/mês</small></div>
            <p className="price-description">Condição especial durante os 6 primeiros meses.</p>
            <p className="price-regular">Preço oficial depois: <strong>R$ 39,99/mês</strong></p>
            <ul className="plan-features"><li>Dashboard financeiro e operacional</li><li>Agenda da equipe e escalas</li><li>Serviços, profissionais e comissões</li><li>App para barbeiros e clientes</li><li>Notificações e suporte em português</li></ul>
            <TrackedLink
              href={pricingDestination}
              className="btn-primary full-width"
              target={adminBaseUrl ? undefined : "_blank"}
              rel={adminBaseUrl ? undefined : "noopener noreferrer"}
              eventName="cta_click"
              eventParams={{ cta_location: "pricing", cta_destination: adminBaseUrl ? "signup" : "whatsapp" }}
            >
              {signupLabel} <span aria-hidden="true">→</span>
            </TrackedLink>
            <small className="pricing-footnote">Válido para as 10 primeiras barbearias. Cancele quando quiser.</small>
          </div>
        </section>

        <section id="faq" className="section-shell faq-section">
          <div className="section-heading centered-heading"><p className="section-eyebrow">Dúvidas frequentes</p><h2>Antes de começar, você precisa ter clareza.</h2></div>
          <div className="faq-list">{faqs.map((faq) => <details className="faq-item" key={faq.question}><summary>{faq.question}<span>+</span></summary><p>{faq.answer}</p></details>)}</div>
        </section>

        <section id="contact" className="final-cta section-shell">
          <div className="final-cta-inner">
            <div><p className="section-eyebrow">Sua próxima melhoria começa aqui</p><h2>Organize a barbearia antes que o próximo horário se perca.</h2><p>Veja o SaaS em funcionamento e descubra se ele combina com a rotina da sua equipe.</p></div>
            <TrackedLink
              href={finalDestination}
              className="btn-primary btn-large"
              target={adminBaseUrl ? undefined : "_blank"}
              rel={adminBaseUrl ? undefined : "noopener noreferrer"}
              eventName="cta_click"
              eventParams={{ cta_location: "final", cta_destination: adminBaseUrl ? "signup" : "whatsapp" }}
            >
              {signupLabel} <span aria-hidden="true">→</span>
            </TrackedLink>
          </div>
        </section>
      </main>

      <footer className="footer">
        <div className="footer-inner">
          <div><Brand /><p className="footer-tagline">Gestão simples para barbearias que querem crescer.</p></div>
          <div className="footer-links"><a href="#produto">Produtos</a><a href="#beneficios">Benefícios</a><a href="#pricing">Planos</a><a href="#faq">FAQ</a><a href="/politica-de-privacidade">Privacidade</a><a href="/termos-de-uso">Termos de Uso</a><TrackedLink href={whatsappUrl("footer")} target="_blank" rel="noopener noreferrer" eventName="cta_click" eventParams={{ cta_location: "footer", cta_destination: "whatsapp" }}>Contato</TrackedLink></div>
          <div className="footer-bottom"><span>© 2026 Na Régua. Todos os direitos reservados.</span><span>Desenvolvido por Caio Santos.</span></div>
        </div>
      </footer>
      <div className="mobile-sticky-cta">
        <TrackedLink
          href={primaryDestination}
          className="btn-primary full-width"
          target={adminBaseUrl ? undefined : "_blank"}
          rel={adminBaseUrl ? undefined : "noopener noreferrer"}
          eventName="cta_click"
          eventParams={{ cta_location: "mobile-sticky", cta_destination: adminBaseUrl ? "signup" : "whatsapp" }}
        >
          {signupLabel} <span aria-hidden="true">→</span>
        </TrackedLink>
      </div>
    </div>
  );
}
