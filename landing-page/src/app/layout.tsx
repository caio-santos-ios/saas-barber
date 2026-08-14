import type { Metadata } from "next";
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
      <body>{children}</body>
    </html>
  );
}
