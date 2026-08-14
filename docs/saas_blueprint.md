# 🚀 Blueprint para Geração de SaaS (Template AI)

Este documento contém um prompt completo e estruturado em "Fases". Você pode copiar o conteúdo abaixo e colar em uma nova conversa com uma IA para instruí-la a recriar toda esta arquitetura do zero, bastando alterar o **Contexto de Negócio** na introdução.

---

## 📋 Copie e cole a partir daqui para a nova conversa:

**Contexto Geral:**
Você é um Engenheiro de Software Full-Stack Sênior. O meu objetivo é criar um sistema SaaS (Software as a Service) do zero.
O sistema base é estruturado para suportar multi-tenancy básico (cada usuário/empresa gerencia seus próprios dados de forma isolada, vinculados ao seu ID de empresa).

**[INSTRUÇÃO PARA O USUÁRIO: ALTERE O CONTEXTO ABAIXO]**
> **Contexto de Negócio:** Atualmente este SaaS é para [Barbearias]. Eu quero mudar para [CLÍNICAS ODONTOLÓGICAS].
> **Entidades Principais:** Dentistas, Pacientes, Tratamentos, Consultas.

**Stack Tecnológico Obrigatório:**
*   **Backend:** .NET (C#) com Controllers (Web API).
*   **Banco de Dados:** MongoDB (usando padrão Repository e Service).
*   **Frontend:** Angular (Standalone Components, sem ngModules). CSS puro (sem Tailwind/Bootstrap).
*   **Integrações:** Asaas (para gestão de Assinaturas/Cobranças via Cartão de Crédito), ViaCEP (para busca de endereço), MailKit (para envio de emails SMTP).
*   **Segurança:** JWT (JSON Web Tokens) e BCrypt (Hash de senhas). Variáveis sensíveis gerenciadas por `.env` (DotNetEnv).

Por favor, execute o desenvolvimento passo a passo, seguindo rigorosamente as fases abaixo. Me pergunte antes de avançar para a próxima fase.

---

### Fase 1: Setup e Infraestrutura
1. Crie a pasta raiz do projeto.
2. Crie a API em .NET (`api-[contexto]`). Instale os pacotes: `MongoDB.Driver`, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `DotNetEnv`, `MailKit`.
3. Configure o `Program.cs` para carregar o `.env` logo no início, habilitar CORS para o frontend, e configurar autenticação JWT.
4. Crie um arquivo `.env.example` com placeholders para `ASAAS_API_KEY`, `SMTP_PASSWORD` e `JWT_KEY`.
5. Crie a aplicação Angular (`ui-[contexto]`). Configure o roteamento padrão e instale dependências úteis como `ngx-toastr` e `ngx-mask`.
6. Configure o `api.ts` (Axios ou fetch) no frontend com interceptadores para injetar o JWT Token (salvo no localStorage).

### Fase 2: Banco de Dados e Modelos Core
Crie os modelos (Classes C#) na pasta `src/Models` e seus respectivos repositórios genéricos na pasta `src/Repositories` (com interfaces).
Modelos essenciais:
1. `ModelBase` (Id, CreatedAt, UpdatedAt).
2. `User` (Nome, Email, Senha, Role, IdDaEmpresa).
3. `Empresa` (Nome, CNPJ/CPF, Telefone, Endereço, SubscriptionStatus, AsaasCustomerId).
4. `Plan` (Nome, Preço, Descrição, Limites).
5. **Modelos de Negócio** (Baseados no contexto escolhido, ex: Dentista, Paciente, Consulta).

### Fase 3: Serviços Base (SaaS Foundation)
Implemente a lógica de negócios na pasta `src/Services`:
1. `AuthService`: Geração de JWT, Hash com BCrypt, Login, Registro de nova Empresa + Usuário Master, e lógica de Reset de Senha (gerar senha aleatória, salvar no DB e chamar o `EmailService`).
2. `EmailService`: Envio de emails HTML via SMTP usando MailKit.
3. `AsaasService`: Integração HTTP direta com a API do Asaas (Criar Cliente, Criar Assinatura via Cartão de Crédito, Cancelar Assinatura, Obter Faturas).
4. `SubscriptionService`: Orquestra o `AsaasService` com a base de dados (Checkout e Cancelamento, alterando o status da Empresa).

### Fase 4: Controladores (Endpoints)
Crie os Controllers na pasta `src/Controllers` para expor os serviços criados na Fase 3.
*   `AuthController` (/login, /register, /reset-password)
*   `SubscriptionController` (/checkout, /cancel, /invoices)
*   Controladores para as entidades de negócio (CRUD básico associado ao ID da empresa do usuário logado, extraído do JWT).

### Fase 5: Layout e Segurança do Frontend (Angular)
1. Crie o `AuthGuard`. Ele deve verificar:
   - Se não tem token JWT -> redireciona para `/login`.
   - Se tem token mas o `subscriptionStatus` (salvo no login) for diferente de 'Ativa', e a rota não for `/subscription` -> redireciona para `/subscription`.
2. Crie o `DashboardLayout` (Componente de Layout principal). Ele deve ter uma barra lateral (Sidebar) e um Header.
3. **Regra Visual:** Se o status da assinatura for inativo, esconda o `Sidebar` inteiro, deixe a tela ocupando 100% da largura, e adicione um botão "Sair" no header.

### Fase 6: Telas Core do SaaS (Frontend)
Construa os componentes standalone no Angular (HTML + TS + CSS puro):
1. **Login & Recuperação:** Formulário de login e modal para "Esqueci minha senha" (que avisa que a senha foi enviada por e-mail).
2. **Cadastro (Register):** Fluxo para cadastrar a Empresa e o primeiro Usuário. Inclua lógica dinâmica para label de CPF/CNPJ com máscara.
3. **Assinaturas (/subscription):** Tela que lista os planos disponíveis. Se a assinatura não estiver ativa, exiba os planos e um formulário de cartão de crédito. Se estiver ativa, mostre histórico de faturas e um botão vermelho (com confirmação em modal) para "Cancelar Assinatura".
4. **Configurações (/settings):** Tela para atualizar dados da empresa. Inclua busca automática de CEP consultando a API pública `viacep.com.br`.
5. **Dashboard (/dashboard):** Exiba métricas principais, faturamento do período (com filtro de datas) e tabela com a agenda do dia (Data/Hora, Cliente, Profissional, Status).

### Fase 7: Telas de Negócio (CRUDs)
Construa as telas para gerenciar as entidades específicas do contexto (Profissionais, Clientes, Agendamentos, Serviços, etc).
1. Todas as telas devem seguir um padrão visual: Header com botão "+ Novo", Tabela para listagem, Modais padronizados para criação/edição e Modais de confirmação (ex: "Tem certeza que deseja deletar?").
2. Sempre garanta responsividade (Mobile First). Nada de scroll horizontal indesejado.

---
**[INSTRUÇÃO FINAL DA IA]**
Após eu colar esse prompt, inicie confirmando as entidades de negócio e o layout de banco de dados, e peça aprovação para iniciar a **Fase 1**.
