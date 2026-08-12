# api-barber — API de Negócio (.NET)

## Visão Geral

O **api-barber** é o backend do SaaS APP Barbearia + Web (Versão 1). É a camada central que orquestra todo o negócio: autenticação e autorização, agendamentos, gestão da barbearia (serviços, barbeiros, escala), notificações, planos e assinatura, além das integrações externas com a **Asaas** (cobrança recorrente e webhooks) e o **Firebase** (auth e push).

O projeto é multi-tenant: toda consulta e escrita de dados é filtrada pelo `barbershopId` do usuário autenticado, garantindo que uma barbearia nunca acesse dados de outra.

## Tecnologias

| Item | Escolha | Observação |
|---|---|---|
| Plataforma | **.NET** | Backend da API REST |
| Banco de dados | **MongoDB** | Persistência das collections (ver modelagem) |
| Autenticação | Firebase Auth + JWT próprio | Login Google/e-mail → troca por JWT da API |
| Pagamentos | Asaas (Handler) | Customer, cobrança recorrente, webhook |
| Notificações | Firebase Cloud Messaging | Push para o app, via handler |
| Idioma/estilo | C# | Arquitetura em camadas descrita abaixo |

## Arquitetura (do cartão "Arquitetura na API")

O código segue a organização em pastas:

| Pasta | Responsabilidade |
|---|---|
| `Middleware` | Guarda de acesso: só libera o request para o controller se o plano estiver ativo ou vencido há no máximo 5 dias; valida papel e token JWT |
| `Interfaces` | Contratos `IService` e `IRepository`, viabilizando injeção de dependência e testabilidade |
| `Controllers` | Endpoints da API; aplicam validações e delegam à service |
| `Services` | Regra de negócio de cada entidade; únicas chamadoras dos repositórios; relacionamentos são resolvidos via service, nunca via repository |
| `Repositories` | Acesso direto ao MongoDB (AppDbContext) |
| `Models` | Modelos das collections (todas herdam de `ModelBase`: id, deleted, createdAt, createdBy, updatedAt, updatedBy) |
| `Requests` | DTOs/requests das collections |
| `Handlers` | Serviços externos: `AsaasHandler`, `UploadHandler`, `MailHandler`, etc. |
| `Infrastructures` | Conexões externas (AppDbContext.cs, mensageria) |

> **Regra de conversão:** deve existir um MAP para converter Request → Model (ex.: `CreateUserRequest` → `User`), mapeando todos os campos preenchidos no request para o model.

## Fluxo de Chamada

```
UI-barber / App (Flutter) → Middleware → Controllers → Services → Repositories
```

## Endpoints Principais (catálogo — nomes em inglês)

| Módulo | Endpoints |
|---|---|
| Autenticação | `POST /auth/login`, `POST /auth/customers/register`, `POST /auth/admins/register`, `POST /auth/forgot-password`, `PUT /users/me` |
| Agendamentos | `/app/appointments`, `/app/appointments/availability`, `/app/appointments/{id}/status` |
| Gestão da barbearia | `/services`, `/barbers`, `/schedules`, `/web/dashboard`, `/web/appointments` |
| Notificações | `/app/notifications`, `/app/notifications/{id}/read` |
| Planos e assinatura | `/web/invoices`, `/web/plans/upgrade`, `/web/plans/cancel` |
| Webhooks | `POST /webhooks/asaas` |

## Integração Asaas (resumo)

No cadastro do admin são criados o **customer** e a **assinatura recorrente** (R$ 39,99/mês). A cobrança mensal é automática; os webhooks `PAYMENT_RECEIVED`, `PAYMENT_OVERDUE` e `PAYMENT_REFUNDED` atualizam as faturas. No upgrade, a cobrança é calculada proporcional; no cancelamento, o acesso permanece até o fim do ciclo pago, bloqueando ao expirar (dados preservados).

## Documentação Relacionada

- [Documentação do SaaS — seções 8, 9 e 10](../docs/documentation.md)
- [Cartão "Arquitetura na API"](https://trello.com/c/S9zjqzNa/5-arquitetura-na-api)
- [Cartão "Collection MongoDB"](https://trello.com/c/1GELmyiW/4-collection-mongodb)
