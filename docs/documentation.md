# Documentação do SaaS — APP Barbearia + Web (Versão 1)


> Esta documentação consolida todos os cartões do quadro do Trello [**SAAS - APP Barbearia + Web - VERSÃO 1**](https://trello.com/b/nvlYcWDY/saas-app-barbearia-web-vers%C3%A3o-1) e organiza o escopo e os requisitos do produto. O texto foi reescrito e melhorado em relação aos cartões originais para clareza e completude. Versão da documentação: **1.2** — atualizada em 12/08/2026.
> 	**Changelog:**
> 	1. v1.0 (08/08/2026) — criação da documentação a partir dos 3 primeiros cartões.
> 	2. v1.1 (09/08/2026) — seção 8 (Modelagem do Banco de Dados — MongoDB) com ajustes aprovados.
> 	3. v1.2 (12/08/2026) — seção 9 (Arquitetura da API), catálogo de endpoints, autenticação JWT, integração Asaas (webhooks, assinatura, recorrente e cancelamento), coleção `notifications` oficial e referência atualizada com os 5 cartões.
> 	4. v1.3 (12/08/2026) — seção 12 (Landing Page) com 8 seções definidas, `durationMinutes` em `services`/`services_types` e referência atualizada.

# 1. Visão Geral do Produto 
Este projeto consiste em um **SaaS de agendamentos para barbearias**, composto por dois fronts: um **aplicativo mobile (App)** e uma **plataforma web (Web)**. A solução digitaliza a rotina de uma barbearia, permitindo que clientes agendem serviços, que barbeiros gerenciem sua agenda e que o dono da barbearia administre toda a operação — serviços, equipe, escala e plano de assinatura — em um ambiente único e integrado.
O modelo de negócio é de **assinatura (SaaS)** voltado às barbearias, com o plano padrão custando **R$ 39,99/mês**. A plataforma web é utilizada pelo dono da barbearia (admin) para gerir o negócio, enquanto o aplicativo mobile atende dois públicos: os **clientes da barbearia** e os **barbeiros** (com e sem permissões administrativas).
# 2. Escopo do Produto 
O sistema é composto por três interfaces de uso, cada uma com público e responsabilidades próprias:
| Interface | Público | Finalidade principal |
|---|---|---|
| **App Mobile (Cliente)** | Clientes finais da barbearia | Agendar, gerenciar e acompanhar agendamentos; receber notificações |
| **App Mobile (Barbeiro)** | Barbeiros da equipe | Visualizar a agenda pessoal e gerenciar o status dos agendamentos; o barbeiro admin também gerencia serviços e barbeiros |
| **Web (Admin / Dono)** | Proprietário da barbearia | Gerir a operação completa: dashboard, plano, escala, serviços, barbeiros e agendamentos |

**Multi-tenancy:** cada barbearia assinante (tenant) possui seu próprio ambiente — barbeiros, serviços, escala e agendamentos — caracterizando um SaaS multi-tenant.
## 2.1 Papéis e Permissões
| Papel | Onde existe | Acesso no App | Acesso na Web |
|---|---|---|---|
| **Cliente** | Apenas App | Home, perfil, meus agendamentos | Não possui |
| **Barbeiro (não admin)** | App + Web (cadastrado pelo admin) | Home, perfil, agendamentos (próprios) | Sem acesso de gestão |
| **Barbeiro Admin** | App + Web | Home, perfil, agendamentos, serviços, barbeiros | Sem acesso de gestão |
| **Admin da barbearia (dono)** | Web | Não possui (opera pelo Web) | Dashboard, painel do plano, escala, serviços, barbeiros, agendamentos |

## 2.2 Fluxos-chave do Produto
1. **Aquisição da barbearia:** o dono se cadastra na Web (com CPF ou CNPJ), contrata o plano e configura serviços, barbeiros e escala.
2. **Inclusão de barbeiros:** o admin cadastra os barbeiros na Web; o sistema envia por e-mail as credenciais de primeiro login do barbeiro no App.
3. **Agendamento pelo cliente:** o cliente baixa o App, cria sua conta e agenda serviços, recebendo confirmação por notificação push.
4. **Execução do serviço:** o barbeiro visualiza seus agendamentos do dia, marca como "fazendo" e "feito"; as alterações disparam notificações ao cliente e ao admin.
> **Regra de segurança transversal (cartão "Requisitos/Telas do App"):** sempre usar o ID do usuário logado para exibir e buscar informações de agendamentos, garantindo que cada barbeiro visualize apenas sua própria agenda e que o acesso aos dados seja sempre contextualizado pela sessão autenticada. Deve verificar se o plano tá ativo (se ta pago o mês), só assim deve ser liberado.

# 3. Stack Tecnológica 
| Camada | Tecnologia | Uso |
|---|---|---|
| Aplicativo mobile | **Flutter** | App único para iOS e Android (cliente e barbeiros) |
| Backend / API | **.NET** | Serviços de negócio, autenticação e integrações |
| Banco de dados | **MongoDB** | Persistência de dados (usuários, agendamentos, serviços etc.) |
| Frontend web | **Angular** | Painel administrativo do dono da barbearia |
| Pagamentos / Assinatura | **Asaas** | Cobrança recorrente do plano, emissão de faturas, upgrade e cancelamento |
| Notificações e autenticação | **Firebase** | Firebase Cloud Messaging (push) e Firebase Auth (login Google, e-mail/senha, recuperação de senha) |

# 4. Requisitos Funcionais — App Mobile (Cliente) 
## 4.1 Autenticação
**Login:** o cliente pode autenticar-se de duas formas: via **Google** (OAuth) ou com **credenciais cadastradas** (e-mail e senha). No fluxo de credenciais, e-mail e senha são obrigatórios.
**Cadastro:** o formulário contém os campos **nome, e-mail, WhatsApp, senha e confirmação de senha**, além do aceite dos **termos e condições**. Campos obrigatórios: nome, e-mail e senha.
**Recuperação de senha ("Esqueci minha senha"):** tela com campo de e-mail obrigatório; o sistema envia ao e-mail do usuário um **link para redefinição de senha**.
## 4.2 Tela Home
- **Header:** foto de perfil à esquerda e ícone de **notificações** à direita.
- **Card de boas-vindas** personalizado com o nome do cliente.
- **Card do próximo agendamento**, com destaque para data, horário, serviço e barbeiro.
- **4 cards de indicadores:** total de agendamentos, agendamentos cancelados, agendamentos em andamento ("fazendo") e agendamentos concluídos ("feitos").
- **Footer de navegação:** Home, Meus Agendamentos e Perfil.
## 4.3 Tela Perfil
Foto de perfil ampliada, com ação de toque para **editar ou remover**, além das **informações pessoais** com opções de cadastrar e atualizar.
## 4.4 Tela Meus Agendamentos
- **Listagem** de todos os agendamentos do cliente.
- **Adicionar** um novo agendamento.
- **Editar** um agendamento existente.
- **Excluir** um agendamento.
- **Filtrar** agendamentos (por data, status, serviço etc.).
- **Editar** agendamentos em rascunho.
- **Cancelar** agendamentos não finalizados.
**Notificações automáticas do cliente:**
| Evento | Notificados |
|---|---|
| Cliente marca novo agendamento | Barbeiro e Admin |
| Cliente cancela agendamento | Barbeiro e Admin |
| Agendamento finalizado | Cliente e Admin |

# 5. Requisitos Funcionais — App Mobile (Barbeiro não Admin) 
## 5.1 Autenticação
O login é realizado com **e-mail e senha** (ambos obrigatórios), recebidos por e-mail no primeiro acesso (credenciais enviadas pelo admin). A **recuperação de senha** é idêntica ao fluxo do cliente: campo de e-mail obrigatório e envio de link por e-mail.
## 5.2 Tela Home
- **Header:** foto de perfil à esquerda e ícone de notificações à direita.
- **Card de boas-vindas.**
- **4 cards de indicadores:** total de agendamentos, cancelados, em andamento ("fazendo") e concluídos ("feitos").
- **Todos os agendamentos do dia** do barbeiro.
- **Footer de navegação:** Home, Agendamentos e Perfil.
## 5.3 Tela Perfil
Foto de perfil ampliada com opção de **editar/remover**, além das informações pessoais com opção de cadastrar e atualizar.
## 5.4 Tela Agendamentos
- **Listagem** dos agendamentos vinculados ao ID do usuário logado (regra transversal: sempre filtrar pelo usuário logado).
- **Filtragem** dos agendamentos do usuário logado.
# 6. Requisitos Funcionais — App Mobile (Barbeiro Admin) 
## 6.1 Autenticação
Idêntica ao barbeiro não admin: login com **e-mail e senha** obrigatórios e recuperação de senha por link enviado ao e-mail.
## 6.2 Tela Home
- **Header:** foto de perfil à esquerda e ícone de notificações à direita.
- **Card de boas-vindas.**
- **4 cards de indicadores:** total, cancelados, fazendo e feitos.
- **Todos os agendamentos do dia.**
- **Footer de navegação:** Home, Agendamentos, **Serviços**, **Barbeiros** e Perfil.
## 6.3 Tela Agendamentos
- **Listagem** dos agendamentos.
- **Filtragem** dos agendamentos.
## 6.4 Tela Serviços
- **Listagem** dos serviços da barbearia.
- **Adicionar, editar e excluir** serviços.
- **Filtragem** dos serviços.
## 6.5 Tela Barbeiros
- **Listagem** de todos os barbeiros da barbearia.
- **Adicionar, editar e excluir** barbeiros.
- **Filtragem** dos barbeiros.
- **Envio de e-mail com as credenciais** do novo barbeiro para o primeiro login.
## 6.6 Tela Perfil
Foto de perfil ampliada com opção de editar/remover e informações pessoais com opção de cadastrar e atualizar.
# 7. Requisitos Funcionais — Plataforma Web (Admin / Dono da Barbearia) 
## 7.1 Autenticação
**Login:** campos de **e-mail e senha**, ambos obrigatórios.
**Cadastro:** o formulário do dono da barbearia contém: **e-mail, nome da barbearia, tipo de pessoa (Física/Jurídica), documento (CPF/CNPJ), senha, confirmação de senha** e aceite dos **termos e condições**.
**Recuperação de senha:** campo de e-mail obrigatório com envio de link de redefinição por e-mail.
## 7.2 Dashboard
O dashboard deve apresentar **métricas que façam sentido para um dono de barbearia**. Sugestões de indicadores que complementam os cards já previstos no App: faturamento do período, agendamentos por dia/semana, serviços mais agendados, barbeiros mais ocupados, taxa de cancelamento e receita por serviço.
## 7.3 Painel do Plano (Assinatura)
- Exibição das **faturas pagas** do plano.
- Opção de **upgrade de plano**, caso exista um plano superior ao contratado.
- Opção de **cancelar o plano** contratado.
> **Integração com Asaas:** a gestão do plano deve usar cobrança recorrente, geração de faturas, webhook de confirmação de pagamento (liberação do acesso) e suporte a upgrade/cancelamento de assinatura no plano contratado do Asaas.

## 7.4 Escala dos Barbeiros
Cadastro de **dias da semana** com **horário inicial e horário final** de trabalho (por barbeiro ou escala geral da barbearia), servindo de base para a disponibilidade de horários no agendamento.
## 7.5 Tela Serviços
- **Listagem** dos serviços.
- **Adicionar, editar e excluir** serviços.
- **Filtragem** dos serviços.
## 7.6 Tela Barbeiros
- **Listagem** de todos os barbeiros.
- **Adicionar, editar e excluir** barbeiros.
- **Filtragem** dos barbeiros.
# 8. Modelagem do Banco de Dados — MongoDB 
## 8.1 Convenção Base — ModelBase
Todas as coleções herdam da **ModelBase**, que garante soft delete e trilha de auditoria em todo o sistema:
| Campo | Tipo | Descrição |
|---|---|---|
| `id` | ObjectId / string | Identificador único do documento |
| `deleted` | bool | Soft delete — nunca excluir fisicamente; consultas sempre filtram `deleted: false` |
| `createdAt` | datetime | Data de criação do documento |
| `createdBy` | string | ID do usuário que criou |
| `updatedAt` | datetime | Data da última atualização |
| `updatedBy` | string | ID do usuário que atualizou por último |

## 8.2 Papéis de Usuário
O sistema possui **3 tipos de usuários**, representados pelo enum `role_user_enum`:
| Valor do enum | Papel | Onde atua |
|---|---|---|
| `admin` | Barbeiro admin (gerencia serviços e barbeiros) | App (telas de serviços e barbeiros) |
| `barber` | Barbeiro comum (gerencia apenas a própria agenda) | App (agendamentos próprios) |
| `customer` | Cliente da barbearia | App (agendamentos) |

> **Importante:** a senha em `users` é sempre armazenada **hasheada** (bcrypt/argon2), nunca em texto puro. A recuperação de senha é gerenciada pelo Firebase Auth. Quem gerencia a barbearia (dono/admin do Web) também é um usuário do sistema; sua identificação pode ser representada por um flag `isOwner: bool` em `users` ou por papel dedicado, conforme decisão da equipe.

## 8.3 barbershops (tenants)
Coleção das barbearias assinantes do SaaS. Cada tenant possui seu ambiente isolado de serviços, escala, barbeiros e agendamentos.
| Campo | Tipo | Descrição |
|---|---|---|
| `name` | string | Nome da barbearia |
| `typePerson` | enum "F" / "J" | Pessoa física ou jurídica |
| `document` | string | CPF ou CNPJ (validar formato) |
| `email` | string | E-mail de contato do dono (cadastro e recuperação de senha) |
| `phone` / `whatsApp` | string | Contato da barbearia |
| `address` | subdocumento | rua, número, bairro, cidade, estado, CEP |
| `logo` | string | Foto/logo da barbearia (exibida no App do cliente) |
| `planId` | string | Plano contratado (vincula com a coleção `plans`) |
| `asaasCustomerId` | string | ID do cliente na Asaas para cobrança recorrente |
| `subscriptionStatus` | enum | `ativa`, `inadimplente`, `cancelada`, `bloqueada` — controla o acesso ao sistema conforme o pagamento |
| `active` | bool | Ativação/bloqueio da barbearia |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

## 8.4 users
Coleção única para todos os papéis do sistema (admin, barber e customer).
| Campo | Tipo | Descrição |
|---|---|---|
| `name` | string | Nome completo (obrigatório) |
| `email` | string | E-mail único (obrigatório) |
| `whatsApp` | string | **Obrigatório para clientes** (requisito do App); útil para lembretes futuros |
| `role` | role_user_enum | `admin`, `barber` ou `customer` |
| `dateOfBirth` | date | Data de nascimento |
| `password` | string | Senha **hasheada** (nunca em texto puro) |
| `firebaseUid` | string | Vincula o usuário ao Firebase Auth (login Google vs. e-mail/senha) |
| `photo` | string | Foto de perfil |
| `document` | string | CPF (barbeiros/admin) |
| `barbershopId` | string | Barbearia do usuário (isolamento multi-tenant) |
| `passwordResetRequired` | bool | Força redefinição de senha no primeiro login (fluxo de credenciais enviadas por e-mail ao barbeiro) |
| `active` | bool | Ativação (barbeiro desligado não perde histórico) |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

> O campo `subscriptionPlan: bool` original foi **movido para a coleção ****`barbershops`** (via `planId` + `subscriptionStatus`), pois quem assina o plano é o dono da barbearia, não o barbeiro ou cliente. Um boolean também não representa um plano com múltiplas opções.

## 8.5 plans
Coleção que define os planos do SaaS, necessária para suportar o requisito de **upgrade de plano** do painel Web.
| Campo | Tipo | Descrição |
|---|---|---|
| `name` | string | Nome do plano (ex.: Básico, Profissional) |
| `description` | string | Descrição e diferenciais do plano |
| `price` | decimal | Valor mensal (ex.: 39,99) |
| `asaasPlanId` | string | Identificador do plano/assinatura na Asaas |
| `level` | int | Ordem hierárquica (define quais upgrades são possíveis) |
| `active` | bool | Plano disponível para contratação |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

## 8.6 services_types (catálogo de serviços)
| Campo | Tipo | Descrição |
|---|---|---|
| `name` | string | Nome do tipo de serviço (ex.: Corte, Barba, Sobrancelha) |
| `description` | string | Descrição do serviço |
| `duration` | int | Duração em minutos |
| `value` | decimal | Valor base do serviço |
| `barbershopId` | string | Isolamento multi-tenant |
| `category` | string | Categoria (ex.: Cabelo, Barba) — facilita filtros e futuras seções do App |
| `active` | bool | Desativar sem excluir (preserva histórico de agendamentos) |
| `durationMinutes` | int | Opcional: duração padrão dos serviços deste tipo, quando não definida em `services` |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

## 8.7 services (associação serviço ↔ barbeiro)
Vincula cada serviço do catálogo a um barbeiro, permitindo que a mesma barbearia ofereça o mesmo serviço com profissionais diferentes.
| Campo | Tipo | Descrição |
|---|---|---|
| `serviceTypeId` | string | Vínculo com `services_types` |
| `barberId` | string | Barbeiro que executa o serviço |
| `barbershopId` | string | Isolamento multi-tenant |
| `active` | bool | Barbeiro pode sair da barbearia sem apagar vínculos históricos |
| `price` | decimal | Opcional: valor específico do barbeiro, caso difira do valor base |
| `commission` | decimal | Opcional: percentual/valor de repasse ao barbeiro (base para futura funcionalidade) |
| `durationMinutes` | int | **Adicionado (12/08):** duração estimada do serviço em minutos; sobrepõe o `intervalMinutes` da escala quando preenchido, permitindo serviços de durações diferentes (ex.: corte 30 min vs. barba + corte 60 min) na mesma agenda |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

## 8.8 schedules (escala de trabalho)
Escala dos barbeiros: dias e horários de trabalho, base para a disponibilidade de horários no agendamento.
| Campo | Tipo | Descrição |
|---|---|---|
| `day` | enum | "seg", "ter", "qua", "qui", "sex", "sab", "dom" — enum evita valores inconsistentes |
| `startHour` | timespan | Horário de início |
| `endHour` | timespan | Horário de fim |
| `intervalMinutes` | int | Duração do intervalo de atendimento (ex.: 30 min) — necessário para gerar horários disponíveis |
| `notes` | string | Observações |
| `barberId` | string | Barbeiro da escala |
| `barbershopId` | string | **Adicionado:** sem este campo não há isolamento multi-tenant nas escalas |
| `active` | bool | Desativar escala sem excluir |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

## 8.9 appointments (agendamentos)
| Campo | Tipo | Descrição |
|---|---|---|
| `date` | datetime | **Ajustado:** data e hora em campo único de datetime — string separada impede comparações e detecção eficiente de conflitos de horário |
| `hour` | string | Horário formatado para exibição (ex.: "14:30") |
| `notes` | string | Observações do cliente |
| `cancelNotes` | string | Motivo do cancelamento |
| `status` | enum | `rascunho`, `marcado`, `cancelado`, `finalizado` (mantido conforme cartão); registrar timestamps por status (`statusDate`) para relatórios |
| `barberId` | string | Barbeiro do agendamento |
| `customerId` | string | Cliente do agendamento |
| `serviceId` | string | Vínculo serviço-barbeiro |
| `serviceTypeId` | string | **Adicionado:** tipo de serviço — garante relatórios de "serviços mais agendados" mesmo após edições/exclusões |
| `barbershopId` | string | Isolamento multi-tenant |
| `value` | decimal | **Adicionado:** valor do agendamento no momento da criação (histórico financeiro não muda se o preço do serviço for editado depois) |
| `customerName` / `serviceTypeName` | string | **Adicionado:** snapshot denormalizado no momento do agendamento (padrão SaaS: o histórico não muda se o usuário editar seus dados depois) |
| `paymentStatus` / `asaasPaymentId` | enum / string | Opcional, para futura cobrança do serviço pelo cliente (pix/boleto/cartão) |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

## 8.10 invoices (faturas do plano)
| Campo | Tipo | Descrição |
|---|---|---|
| `date` | date | Data de emissão |
| `dueDate` | date | Data de vencimento |
| `paidAt` | date | **Adicionado:** data real do pagamento |
| `value` | decimal | Valor da fatura |
| `status` | enum | `em_aberto`, `pago`, `vencido` |
| `paymentMethod` | enum | **Adicionado:** "pix", "boleto", "cartao", "debito_em_conta" |
| `description` | string | **Adicionado:** ex.: "Plano mensal barbearia — ago/2026" |
| `asaasInvoiceId` | string | **Adicionado:** vínculo com a fatura na Asaas — sem ele não é possível reconciliar o webhook de pagamento |
| `asaasCustomerId` | string | **Adicionado:** reforço do vínculo com o tenant na Asaas |
| `barbershopId` | string | Isolamento multi-tenant |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

## 8.10b notifications (notificações do app) — **obrigatória**
Coleção que alimenta o ícone de notificação presente nas telas Home do App (seções 4, 5 e 6 dos requisitos), persistindo cada notificação disparada pelos eventos de agendamento, cancelamento e finalização. As notificações push seguem sendo enviadas via Firebase Cloud Messaging, mas o histórico visível no app vem desta coleção.
| Campo | Tipo | Descrição |
|---|---|---|
| `type` | enum | `agendamento_novo`, `agendamento_cancelado`, `agendamento_finalizado`, `assinatura`, `aviso_sistema` |
| `title` | string | Título exibido no card (ex.: "Novo agendamento confirmado") |
| `message` | string | Texto completo da notificação |
| `userId` | string | Destinatário da notificação |
| `targetRole` | enum | `customer`, `barber`, `admin` — filtro do painel de notificações |
| `relatedAppointmentId` | string | Opcional: vínculo com o agendamento gerador (permite navegar ao detalhe) |
| `read` | bool | Marcar como lida após visualização no app |
| `barbershopId` | string | Isolamento multi-tenant |
| *(ModelBase)* | — | id, deleted, createdAt, createdBy, updatedAt, updatedBy |

> **Eventos que disparam notificações:** novo agendamento (notifica barbeiro e admin), cancelamento pelo cliente (notifica barbeiro e admin) e finalização do serviço (notifica cliente e admin), conforme a tabela da seção 4.4.

## 8.11 Índices Sugeridos
| Coleção | Índice | Motivo |
|---|---|---|
| `barbershopId` | Isolamento multi-tenant em todas as consultas |  |
| `appointments` | `customerId`, `barberId`, `date` | Agenda do cliente/barbeiro e busca por data |
| `appointments` | `barbershopId`, `barberId`, `date`, `status` | Detecção de conflitos de horário |
| `notifications` | `userId`, `read`, `createdAt` | Lista de notificações do usuário (badge e painel) |
| `users` | `email`, `firebaseUid` | Login e recuperação de senha únicos |
| `invoices` | `barbershopId`, `status`, `dueDate` | Job de vencimento e listagem de faturas |
| `services` | `serviceTypeId`, `barberId`, `barbershopId` | Listagem com filtros |

## 8.12 Resumo dos Ajustes Aplicados à Modelagem Original
1. **`barbershopId`** adicionado a **`schedules`** — garantia do isolamento multi-tenant.
2. **Vínculos Asaas adicionados a ****`invoices`** (`asaasInvoiceId`, `asaasCustomerId`) — conciliação de webhooks de pagamento.
3. **`subscriptionPlan: bool`** removido de **`users`** e substituído por `planId` + `subscriptionStatus` em `barbershops` — quem assina é o dono da barbearia.
4. **Campos de contato adicionados a ****`barbershops`** (email, phone, address, logo) e **`whatsApp`**** em ****`users`** — requisitos do cadastro.
5. **Nova coleção ****`plans`** — suporta o requisito de upgrade de plano do painel Web.
6. **`hour`**** separado de ****`date`** como string de exibição — datetime único para comparações e ordenação.
7. **Snapshot denormalizado em ****`appointments`** (`value`, `serviceTypeId`, nomes) — histórico estável.
8. **Papéis definidos como 3 enums:** `admin` (barbeiro admin), `barber` e `customer`.
9. **Campos opcionais de crescimento:** `commission` em `services`, `paymentStatus` em `appointments`, `category` em `services_types`.
10. **`password_reset_tokens`**** não será implementada**, conforme decisão do produto — a recuperação de senha é gerenciada integralmente pelo Firebase Auth.
# 9. Arquitetura da API 
Esta seção documenta a estrutura interna da API desenvolvida em **.NET**, conforme definido no cartão [Arquitetura na API](https://trello.com/c/S9zjqzNa/5-arquitetura-na-api) do Trello.
## 9.1 Organização de Pastas
| Pasta | Responsabilidade |
|---|---|
| Middleware | Verificação do status do plano da barbearia. A requisição somente avança para a controller se o plano estiver ativo ou vencido há no máximo 5 dias. |
| Controllers | Endpoints da API. Recebem a requisição validada e delegam a execução ao service correspondente. |
| Services | Regras de negócio. São chamados pelas controllers ou por outros services. Em caso de relacionamento entre entidades, a consulta deve ser feita ao service da entidade relacionada, e não ao repository. |
| Repositories | Acesso direto ao banco de dados. Somente o service da própria entidade chama o seu repository. |
| Models | Modelos das collections do MongoDB, seguindo a modelagem da seção 8. |
| Requests | DTOs de entrada/saída (Request/Response) das collections. |
| Handlers | Integração com serviços externos: AsaasHandler, UploadHandler, MailHandler, entre outros. |
| Infrastructures | Conexão com o banco de dados (ex.: `AppDbContext.cs`) e chamadas de mensageria. |

## 9.2 Regra de Conversão Request → Model
> Deve existir um mapeamento dedicado para converter cada Request em seu Model correspondente (por exemplo, `CreateUserRequest` → `User`), mapeando todos os campos preenchidos no request e populando o model.
## 9.3 Fluxo de Chamada da API
`UI-barber → Middleware → Controllers → Service → Repository`
O middleware de plano atua como guarda de acesso: caso o plano esteja vencido há mais de 5 dias ou cancelado, a requisição é interrompida antes de alcançar a controller, retornando ao cliente o status da assinatura.
## 9.4 Autenticação e Identificação Multi-tenant (JWT)
A autenticação segue um fluxo híbrido **Firebase Auth + JWT próprio**, que é o padrão recomendado para combinar login social (Google) com autorização na API .NET:
1. **Login social ou credenciais** — o app mobile realiza o login via **Firebase Auth** (Google OAuth ou e-mail/senha). O Firebase valida as credenciais e emite um ID Token.
2. **Troca por JWT da API** — no primeiro acesso, o token do Firebase (`firebaseUid`) é enviado a um endpoint `POST /auth/login`, que consulta a coleção `users` pelo `firebaseUid` (criando o usuário se for o primeiro acesso do cliente via Google) e retorna um **JWT próprio assinado pela API**.
3. **Conteúdo do JWT** — o token deve carregar, no mínimo: `userId`, `role` (`customer`, `barber`, `admin`), `barbershopId` (identificação do tenant) e `firebaseUid`. Assim, o middleware de plano e todos os services identificam o tenant e o papel sem consultar o banco a cada requisição.
4. **Uso nas requisições** — o JWT é enviado no header `Authorization: Bearer ` em todos os endpoints protegidos. A senha em `users` permanece **hasheada (bcrypt/argon2)**, nunca em texto puro, e a recuperação de senha é gerenciada integralmente pelo Firebase Auth.
5. **Validação de papel** — além do middleware de plano, cada controller deve validar se o papel do JWT possui permissão para a ação (ex.: apenas `admin` gerencia serviços e barbeiros), seguindo a matriz da seção 2.1.
> **Regra de segurança transversal:** todas as consultas usam sempre o `barbershopId` e o `userId` extraídos do JWT — nunca valores enviados pelo cliente — garantindo o isolamento multi-tenant e que cada barbeiro visualize apenas sua própria agenda.
## 9.5 Catálogo de Endpoints
Catálogo mínimo de endpoints da API, organizado por módulo. Os prefixos `/app/` atendem o App Mobile (Flutter) e `/web/` o painel Web (Angular); a autenticação é comum a ambos.
### Autenticação e usuários
| Método | Endpoint | Responsável | Descrição |
|---|---|---|---|
| POST | `/auth/login` | Cliente, barbeiro, admin | Troca do token Firebase por JWT da API (retorna token, papel e barbershopId) |
| POST | `/auth/customers/register` | Público | Cria conta do cliente (`CreateUserRequest` → `User`) e vincula ao `barbershopId` da barbearia escolhida |
| POST | `/auth/admins/register` | Público | Cria a barbearia (tenant), o usuário admin e a assinatura na Asaas |
| POST | `/auth/forgot-password` | Público | Aciona o envio do link de redefinição via Firebase Auth |
| PUT | `/users/me` | Autenticado | Atualiza foto e informações pessoais do usuário logado |

### Agendamentos (App — cliente e barbeiro)
| Método | Endpoint | Responsável | Descrição |
|---|---|---|---|
| GET | `/app/appointments` | Cliente, barbeiro, admin | Lista os agendamentos do usuário logado, com filtros de data/status/serviço |
| POST | `/app/appointments` | Cliente | Cria o agendamento com validação de disponibilidade (escala + conflitos de horário); dispara notificação ao barbeiro e ao admin |
| GET | `/app/appointments/{id}` | Cliente, barbeiro, admin | Detalhe do agendamento (sempre validado contra o usuário logado) |
| PUT | `/app/appointments/{id}` | Cliente | Edita agendamento marcado, respeitando a janela de cancelamento |
| DELETE | `/app/appointments/{id}` | Cliente | Cancela agendamento (com `cancelNotes`); notifica barbeiro e admin |
| PATCH | `/app/appointments/{id}/status` | Barbeiro, admin | Atualiza o status ("fazendo" / "feito"); finalização notifica cliente e admin |
| GET | `/app/appointments/availability` | Cliente | Retorna horários disponíveis para barbeiro/serviço/data conforme escala e conflitos |

### Gestão da barbearia (App admin e Web)
| Método | Endpoint | Responsável | Descrição |
|---|---|---|---|
| CRUD | `/services` | Admin (App + Web) | Criar, listar, editar e excluir (soft delete) serviços |
| CRUD | `/barbers` | Admin (App + Web) | Criar, listar, editar e excluir barbeiros, com envio de credenciais por e-mail |
| CRUD | `/schedules` | Admin (Web) | Gerir a escala dos barbeiros (dias, horários e intervalos) |
| GET | `/web/dashboard` | Admin | Métricas do dashboard: faturamento, agendamentos por período, serviços mais agendados, barbeiros mais ocupados, taxa de cancelamento |
| CRUD | `/web/appointments` | Admin | Visão geral dos agendamentos da barbearia (listar, filtrar, cancelar) |

### Notificações (App)
| Método | Endpoint | Responsável | Descrição |
|---|---|---|---|
| GET | `/app/notifications` | Cliente, barbeiro, admin | Lista as notificações do usuário logado (filtro de não lidas) |
| PATCH | `/app/notifications/{id}/read` | Cliente, barbeiro, admin | Marca a notificação como lida |

### Planos e assinatura (Web)
| Método | Endpoint | Responsável | Descrição |
|---|---|---|---|
| GET | `/web/invoices` | Admin | Lista as faturas do plano da barbearia (vínculo Asaas) |
| POST | `/web/plans/upgrade` | Admin | Solicita upgrade de plano (cria/atualiza a assinatura na Asaas) |
| POST | `/web/plans/cancel` | Admin | Cancela a assinatura (mantém acesso até o fim do ciclo pago) |

### Webhooks
| Método | Endpoint | Responsável | Descrição |
|---|---|---|---|
| POST | `/webhooks/asaas` | Asaas | Recebe os eventos de cobrança (`PAYMENT_RECEIVED`, `PAYMENT_OVERDUE`, `PAYMENT_REFUNDED`) com validação do `access_token` no header |

# 10. Integração com a Asaas — Assinatura, Pagamento Recorrente e Cancelamento 
Esta seção detalha o ciclo de vida da assinatura da barbearia, cobrindo a contratação do plano, a cobrança recorrente mensal e o cancelamento, além dos webhooks que sincronizam a Asaas com o sistema.
## 10.1 Contrato do Plano (Assinatura Recorrente)
O plano do SaaS é comercializado como **assinatura recorrente** na Asaas, criada no momento do cadastro do dono da barbearia no painel Web (seção 7.1):
1. **Criação do customer:** ao cadastrar a barbearia, o `CadastroWebRequest` aciona o `AsaasHandler.createCustomer()` — o retorno (`asaasCustomerId`) é persistido em `barbershops`.
2. **Escolha do plano:** o sistema consulta a coleção `plans` (seção 8.5) para listar os planos disponíveis; o `planId` escolhido e o `subscriptionStatus` inicial (`ativa`) são gravados em `barbershops`.
3. **Criação da assinatura:** o `AsaasHandler.createSubscription()` cria a assinatura recorrente na Asaas vinculada ao `asaasCustomerId`, informando o `chargeType` (cartão de crédito como padrão para cobrança automática mensal), valor, ciclo de cobrança (`MONTHLY`) e data de vencimento do primeiro pagamento.
4. **Retorno ao dono:** o painel Web exibe a confirmação da contratação com o valor, ciclo e data do primeiro débito; o status aparece como "ativa" mesmo antes da primeira cobrança (acesso liberado), sendo regularizado pelo webhook de pagamento.
## 10.2 Pagamento Recorrente Mensal
| Etapa | Responsável | O que acontece |
|---|---|---|
| Geração da cobrança | Asaas (automático) | A cada ciclo mensal, a Asaas gera automaticamente a cobrança recorrente com base na assinatura criada |
| Débito/cobrança | Asaas | Débito automático no cartão de crédito; em caso de recusa, a cobrança segue o status `PENDENTE` |
| Confirmação | Webhook | `PAYMENT_RECEIVED` é disparado e o `PaymentReceivedHandler` localiza a fatura pelo `asaasInvoiceId`, atualiza `status` para `pago`, grava `paidAt` e `paymentMethod` |
| Liberação de acesso | Middleware | O middleware de plano (seção 9.1) continua permitindo requisições enquanto o plano estiver ativo ou vencido há no máximo 5 dias |
| Inadimplência | Job + webhook | Após 5 dias de vencimento sem pagamento (`PAYMENT_OVERDUE`), `subscriptionStatus` vira `inadimplente` e o middleware bloqueia o acesso da barbearia até a regularização |

Os eventos de cobrança registrados na Asaas são espelhados na coleção `invoices` (seção 8.10): cada cobrança gera uma fatura com `asaasInvoiceId`, `asaasCustomerId`, `dueDate`, `value` e `description` (ex.: "Plano mensal barbearia — ago/2026").
> **Webhooks Asaas necessários:** `PAYMENT_RECEIVED` (confirma pagamento e libera acesso), `PAYMENT_OVERDUE` (marca inadimplência) e `PAYMENT_REFUNDED` (estorno). O endpoint `POST /webhooks/asaas` valida a assinatura do webhook (header `access_token`) antes de processar qualquer evento, por segurança.
## 10.3 Upgrade de Plano
O endpoint `POST /web/plans/upgrade` (seção 9.5) realiza a mudança de plano:
1. Valida se o plano destino existe em `plans` e possui nível superior ao atual (o upgrade exige um plano superior ao contratado — downgrade fica para versão futura).
2. Atualiza `barbershops.planId` e chama o `AsaasHandler.updateSubscription()` para alterar o valor da assinatura recorrente, mantendo o mesmo ciclo mensal.
3. As próximas cobranças passam a considerar o novo valor; faturas já emitidas no ciclo corrente não são recalculadas.
## 10.4 Cancelamento do Plano
O endpoint `POST /web/plans/cancel` (seção 9.5) realiza o cancelamento:
1. Confirma a intenção com uma tela de confirmação no Web (evitar clique acidental).
2. Chama o `AsaasHandler.cancelSubscription()` — a assinatura é encerrada na Asaas.
3. `barbershops.subscriptionStatus` passa a `cancelada` e `active` permanece `true` **até o fim do ciclo já pago** — o dono conserva acesso pelos dias que já pagou (princípio de valor pelo ciclo).
4. Ao vencer o ciclo pago, um job noturno verifica as barbearias com `subscriptionStatus: cancelada` e ciclo expirado, alterando `active` para `false` — o middleware passa a bloquear o acesso da barbearia.
5. As faturas do histórico permanecem visíveis no painel do plano (seção 7.3) para fins de registro fiscal do dono.
> **Nota:** o cancelamento da assinatura não exclui os dados da barbearia — serviços, barbeiros, agendamentos e notificações permanecem com soft delete (ModelBase), permitindo reativação em uma futura funcionalidade de reinstalação.
# 11. Sugestões de Melhoria e Pontos de Atenção 
Estas sugestões complementam o escopo original e podem ser avaliadas para a Versão 1 ou para próximas iterações:
1. **Fluxo de agendamento no App (cliente):** os cartões não detalham a jornada de criação do agendamento (escolher barbeiro, serviço, data/hora e confirmar). Vale documentar essa tela com validações de disponibilidade conforme a escala e os agendamentos existentes.
2. **Gestão de status do agendamento:** definir uma máquina de estados clara (rascunho → agendado → em andamento → feito / cancelado) com regras de transição — por exemplo, apenas o barbeiro pode marcar "fazendo".
3. **Janela de cancelamento:** definir se o cliente pode cancelar a qualquer momento ou apenas até X horas antes do horário agendado.
4. **Onboarding do barbeiro:** como o barbeiro recebe credenciais por e-mail, avaliar o envio de um link de primeiro acesso com redefinição obrigatória de senha.
5. **Multi-barbearia no App:** considerar que um cliente pode frequentar barbearias diferentes, associando seus agendamentos à barbearia correta.
6. **Ausência do barbeiro:** definir o comportamento de reagendamento/cancelamento automático quando a escala é alterada.
7. **LGPD:** como são coletados dados pessoais (nome, e-mail, WhatsApp), incluir política de privacidade e previsão de exclusão de conta.
8. **Relatórios no dashboard:** replicar no dashboard web os 4 cards do App (total/cancelados/fazendo/feitos) com filtros de período.
9. **Asaas — assinatura:** validar o suporte a assinatura recorrente (cobrança automática mensal) no plano do Asaas contratado.
10. **Notificações push:** usar tópicos Firebase por barbearia/usuário para direcionar corretamente os avisos de agendamento, cancelamento e conclusão.
# 12. Landing Page 
Página pública de apresentação do SaaS, responsável por converter o dono da barbearia em assinante. A landing page funciona como a porta de entrada do funil de aquisição e deve conter **CTA (call to action) para o cadastro no painel Web** (seção 7.1) em pelo menos 3 pontos da página.
## 12.1 Seções da Página
| # | Seção | Conteúdo e objetivo |
|---|---|---|
| 1 | **Hero** | Título de impacto sobre a digitalização da agenda da barbearia, subtítulo com o valor principal ("sua barbearia organizada em minutos"), imagem de destaque e CTA "Criar conta grátis" |
| 2 | **Como funciona** | 3 passos: criar conta → cadastrar serviços, barbeiros e escala → clientes agendam pelo App |
| 3 | **Benefícios** | Cards com os benefícios: agenda sem conflito, notificações automáticas, relatórios de faturamento e cancelamento, gestão de equipe |
| 4 | **Comparativo: antes vs. depois** | Planilha/agenda de papel vs. dashboard digital — reforço visual do ganho |
| 5 | **Planos e preços** | Destaque do plano padrão (R$ 39,99/mês) com o que está incluso; espaço para futuros planos quando houver upgrade |
| 6 | **Screenshots do produto** | Miniaturas das telas reais do App e do painel Web (usar o protótipo já construído) |
| 7 | **FAQ** | Perguntas frequentes: formas de pagamento, período de teste, cancelamento, suporte |
| 8 | **Rodapé** | Links de cadastro/login, política de privacidade, termos de uso e contato |

## 12.2 Tecnologia — Next.js
A landing page será desenvolvida em **Next.js** (App Router, React), escolhida por três motivos alinhados ao objetivo da página:
1. **SEO por padrão:** renderização do lado do servidor (SSR) garante que os buscadores recebam o HTML completo da página, essencial para ranquear em buscas como "sistema de agendamento para barbearia" — algo que SPAs puras não entregam.
2. **Performance:** geração estática das rotas públicas (SSG) com carregamento otimizado de imagens (`next/image`), fontes e código dividido por rota (code splitting).
3. **Ecossistema:** mesma linguagem do painel Web (TypeScript/React) facilita a manutenção pelo mesmo time e o reaproveitamento do design system.
## 12.3 SEO
| Frente | Requisito |
|---|---|
| Meta tags | Title (até 60 caracteres) e description (até 160) otimizados por rota, via `metadata` API do Next.js |
| Open Graph / Twitter | Imagem e textos compartilháveis para redes sociais (pré-visualização do SaaS) |
| sitemap.xml | Gerado automaticamente, com todas as rotas públicas |
| robots.txt | Permitir indexação do Google e Bing |
| Dados estruturados | JSON-LD do tipo `SoftwareApplication` (nome, descrição, preço, avaliação) para rich results |
| Performance | Meta Core Web Vitals: LCP &lt; 2,5s, CLS &lt; 0,1, INP &lt; 200ms |
| Conteúdo | Textos otimizados para as buscas-alvo: "sistema de agendamento para barbearia", "app para barbearia", "gestão de barbearia" |

## 12.4 Observações Técnicas
1. A landing page é **pública** (não exige autenticação) e deve priorizar velocidade de carregamento.
2. O CTA de cadastro direciona para o fluxo `POST /auth/admins/register` do painel Web, criando automaticamente a barbearia (tenant), o usuário admin e a assinatura recorrente na Asaas (seção 10.1).
3. As perguntas de FAQ sobre cobrança/cancelamento devem ser consistentes com o fluxo da seção 10 (cobrança mensal automática, cancelamento com acesso até o fim do ciclo pago).
# 13. Referência — Cartões Originais no Trello 
| Cartão | Link | Seção da documentação |
|---|---|---|
| Escopo e tecnologias | [https://trello.com/c/1l7GO77Y/2-escopo-e-tecnologias](https://trello.com/c/1l7GO77Y/2-escopo-e-tecnologias) | 1, 2 e 3 |
| Requisitos/Telas do App | [https://trello.com/c/htLZZfxV/1-requisitos-telas-do-app](https://trello.com/c/htLZZfxV/1-requisitos-telas-do-app) | 4, 5 e 6 |
| Requisitos/Tela do web | [https://trello.com/c/bAkTOl0m/3-requisitos-tela-do-web](https://trello.com/c/bAkTOl0m/3-requisitos-tela-do-web) | 7 |
| Collection MongoDB | [https://trello.com/c/1GELmyiW/4-collection-mongodb](https://trello.com/c/1GELmyiW/4-collection-mongodb) | 8 |
| Arquitetura na API | [https://trello.com/c/S9zjqzNa/5-arquitetura-na-api](https://trello.com/c/S9zjqzNa/5-arquitetura-na-api) | 9 |
| Landing Page | [https://trello.com/c/EpG4aFqF/6-landing-page](https://trello.com/c/EpG4aFqF/6-landing-page) | 12 |

