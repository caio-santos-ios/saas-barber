import Link from "next/link";
export default function LegalPage({ html }: { title?: string; html: string }) {
  return (
    <main className="legal-page">
      <div className="legal-page-topbar">
        <Link href="/" className="legal-brand" aria-label="Voltar para a página inicial">
          <span className="brand-mark">NR</span>
          <span>Na Régua</span>
        </Link>
        <Link href="/" className="legal-back">Voltar para o site</Link>
      </div>
      <article className="legal-card">
        <div className="legal-kicker">Na Régua · Informações legais</div>
        <div className="legal-content" dangerouslySetInnerHTML={{ __html: html }} />
      </article>
      <footer className="legal-footer">
        <span>© 2026 Na Régua. Todos os direitos reservados.</span>
        <span><Link href="/politica-de-privacidade">Privacidade</Link> · <Link href="/termos-de-uso">Termos de Uso</Link></span>
      </footer>
    </main>
  );
}
