import type { Metadata } from "next";
import Script from "next/script";
import "./globals.css";

export const metadata: Metadata = {
  title: "SaaS Barbearia | Gestão Completa para o seu Negócio",
  description: "Revolucione a gestão da sua barbearia com o nosso SaaS. Agendamentos, controle de caixa, assinaturas e muito mais em um único lugar.",
  keywords: "barbearia, sistema para barbearia, gestão de barbearia, agendamento online, saas",
  openGraph: {
    title: "SaaS Barbearia | Gestão Completa",
    description: "Revolucione a gestão da sua barbearia com o nosso SaaS. Agendamentos, controle de caixa, assinaturas e muito mais.",
    type: "website",
    locale: "pt_BR",
  }
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR">
      <head>
        <Script
          src="https://www.googletagmanager.com/gtag/js?id=G-3EGKNN013P"
          strategy="afterInteractive"
        />
        <Script id="google-analytics" strategy="afterInteractive">
          {`
            window.dataLayer = window.dataLayer || [];
            function gtag(){dataLayer.push(arguments);}
            gtag('js', new Date());

            gtag('config', 'G-3EGKNN013P');
          `}
        </Script>
      </head>
      <body>{children}</body>
    </html>
  );
}
