# Checklist de publicação — Na Régua

**Status:** preparação para publicação  
**Aplicativo:** Na Régua  
**Modelo:** aplicativo único com seleção de perfil “Sou Cliente” e “Sou Profissional”  
**Responsável:** Caio dos Santos Silva  
**E-mail:** caiodesenvolvedor.fullstack@gmail.com  
**Classificação pretendida:** 16 anos ou mais

> **Aviso:** esta checklist é operacional. As políticas e exigências das lojas podem mudar, portanto confira as telas atuais do Google Play Console e do App Store Connect no momento do envio.

## 1. Informações já confirmadas

| Item | Informação |
|---|---|
| Nome comercial | Na Régua |
| Responsável | Caio dos Santos Silva |
| CPF | 086.306.258-70 |
| Endereço | Rua Joana D’Arc, 57, Teotônio Vilela, Ilhéus/BA |
| Suporte | caiodesenvolvedor.fullstack@gmail.com |
| Notificações | Sim, lembretes e alterações de agendamentos |
| Pagamentos dentro do app | Não; o app possui agenda e gestão de perfis |
| Analytics | Não utiliza analytics |
| Exclusão de conta | Disponível no aplicativo |
| Público | 16 anos ou mais |

## 2. Antes do primeiro envio

Confirme que o aplicativo usa um identificador único de produção, diferente de qualquer versão de teste. Para o Android, preencher **[PREENCHER — applicationId]**. Para o iOS, preencher **[PREENCHER — Bundle ID]**.

Revise o fluxo completo dos dois perfis: cadastro, login, recuperação de senha, seleção de cliente/profissional, troca de barbearia, agenda, criação e cancelamento de agendamento, notificações, logout e exclusão da conta. Teste também permissões negadas e ausência de conexão.

Hospede a Política de Privacidade e os Termos de Uso em URLs públicas. Preencha as URLs abaixo antes de enviar o app:

- Política de Privacidade: **[PREENCHER — URL pública]**
- Termos de Uso: **[PREENCHER — URL pública]**
- Página de suporte: **[PREENCHER — URL ou página pública]**

## 3. Google Play Store

1. Criar ou acessar a conta no [Google Play Console](https://play.google.com/console/).
2. Criar o app com o nome **Na Régua** e selecionar português do Brasil como idioma principal.
3. Definir o identificador Android único e configurar a chave de assinatura do app.
4. Gerar o pacote de produção com `flutter build appbundle --release`.
5. Enviar o arquivo `.aab` para um teste interno.
6. Preencher descrição curta, descrição completa, categoria, ícone e screenshots.
7. Informar que o app permite criar conta e que possui opção de exclusão de conta.
8. Preencher a seção de segurança de dados: cadastro, e-mail, telefone, dados de agendamento, dados técnicos necessários e notificações push, conforme o comportamento real.
9. Declarar que o app não utiliza analytics e não processa pagamentos dentro do app, se isso permanecer verdadeiro na versão enviada.
10. Informar a classificação indicativa pretendida de 16 anos ou mais e responder o questionário da loja.
11. Adicionar a URL pública da Política de Privacidade.
12. Enviar para teste fechado com clientes e profissionais convidados.
13. Corrigir falhas encontradas e solicitar a revisão para produção.

## 4. Apple App Store

1. Criar ou acessar a conta no [Apple Developer Program](https://developer.apple.com/programs/).
2. Criar o app no [App Store Connect](https://appstoreconnect.apple.com/) com o nome **Na Régua**.
3. Criar um Bundle ID único para o aplicativo.
4. Configurar certificados e assinatura de distribuição.
5. Gerar a versão iOS com `flutter build ipa --release` em um ambiente compatível com Xcode.
6. Enviar o build para o App Store Connect e disponibilizá-lo no TestFlight.
7. Preencher nome, subtítulo, descrição, palavras-chave, screenshots e informações de suporte.
8. Informar corretamente os dados coletados no formulário de privacidade da Apple.
9. Declarar o uso de notificações para lembretes e alterações de agendamentos.
10. Informar a URL pública da Política de Privacidade e dos Termos de Uso.
11. Responder o questionário de classificação etária e selecionar a faixa correspondente a 16 anos ou mais.
12. Testar o login de cliente e profissional no TestFlight.
13. Enviar para revisão e acompanhar eventuais solicitações da Apple.

## 5. Material necessário para as fichas das lojas

Prepare um ícone final, screenshots reais do app nos tamanhos solicitados, uma descrição curta, uma descrição completa, e-mail de suporte e URLs públicas. Não use nomes, e-mails, telefones, CPFs ou dados financeiros reais nas screenshots. Substitua os dados de demonstração por dados fictícios antes de enviar as imagens.

## 6. Pontos de atenção para este app

Como o mesmo aplicativo serve cliente e profissional, a descrição das lojas deve explicar claramente que o usuário escolhe o perfil de acesso no login. O texto também deve deixar claro que o Na Régua oferece tecnologia de agenda e organização, enquanto a prestação do serviço de corte, barba ou estética é responsabilidade da barbearia.

A classificação de 16 anos ou mais deve ser coerente com o público configurado nas lojas e com a Política de Privacidade. Se o app permitir usuários menores de 18 anos, revise o tratamento de dados e as regras de consentimento com advogado.

A exclusão de conta precisa ser fácil de localizar e efetivamente apagar ou anonimizar os dados, respeitando as retenções legais descritas na Política de Privacidade. Teste esse fluxo antes do envio.

## Referências oficiais

[Google Play Console](https://play.google.com/console/)  
[Google Play — requisitos para publicação](https://support.google.com/googleplay/android-developer/answer/9859455?hl=pt-BR)  
[Apple Developer Program](https://developer.apple.com/programs/)  
[App Store Connect](https://appstoreconnect.apple.com/)  
[Apple App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)  
[LGPD — Lei nº 13.709/2018](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709.htm)
