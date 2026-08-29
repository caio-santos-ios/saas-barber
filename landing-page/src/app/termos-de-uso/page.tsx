import type { Metadata } from "next";
import LegalPage from "../legal/LegalPage";
import { termos_de_uso } from "../legalContent";

export const metadata: Metadata = {
  title: "Termos de Uso | Na Régua",
  description: "Termos de Uso do aplicativo Na Régua.",
};

export default function TermosDeUsoPage() {
  return <LegalPage title="Termos de Uso — Na Régua" html={termos_de_uso} />;
}
