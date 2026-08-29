import type { Metadata } from "next";
import LegalPage from "../legal/LegalPage";
import { politica_de_privacidade } from "../legalContent";

export const metadata: Metadata = {
  title: "Política de Privacidade | Na Régua",
  description: "Política de Privacidade do aplicativo Na Régua.",
};

export default function PoliticaDePrivacidadePage() {
  return <LegalPage title="Política de Privacidade — Na Régua" html={politica_de_privacidade} />;
}
