import type { Metadata } from "next";
import Script from "next/script";
import "./globals.css";

const siteUrl = "https://saas-barber-xi.vercel.app";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: "Sistema para Barbearia | Agenda, Caixa e Equipe",
  description:
    "Organize agenda, profissionais, comissões e caixa em um único sistema. SaaS de gestão para barbearias com painel Web e apps para clientes e barbeiros.",
  keywords: [
    "sistema para barbearia",
    "gestão de barbearia",
    "agendamento para barbearia",
    "app para barbearia",
    "agenda de barbeiro",
  ],
  alternates: { canonical: "/" },
  openGraph: {
    title: "SaaS Barbearia | Gestão simples para crescer",
    description:
      "Agenda, equipe, comissões e caixa em um só lugar — com painel para o administrador e apps para barbeiros e clientes.",
    url: siteUrl,
    siteName: "SaaS Barbearia",
    locale: "pt_BR",
    type: "website",
    images: [
      {
        url: "/dashboard-preview.png",
        width: 1024,
        height: 498,
        alt: "Dashboard do SaaS Barbearia",
      },
    ],
  },
  twitter: {
    card: "summary_large_image",
    title: "SaaS Barbearia | Gestão simples para crescer",
    description:
      "Agenda, equipe, comissões e caixa em um só lugar para sua barbearia.",
    images: ["/dashboard-preview.png"],
  },
  robots: { index: true, follow: true },
  verification: {
    google: "csKkXM-YJfmBVvWbHChV-NBz7UTyDiKPMa0F9zbaDBQ",
  },
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
