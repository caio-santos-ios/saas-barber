# ui-barber — Painel Web Administrativo (Angular)

## Visão Geral

O **ui-barber** é o painel administrativo web do SaaS APP Barbearia + Web (Versão 1), utilizado pelo **dono da barbearia (admin)** para gerir toda a operação do negócio. É a primeira interface que o cliente do SaaS encontra: por ela ele cria a conta, configura sua barbearia, contrata o plano de assinatura e administra serviços, barbeiros, escala e agendamentos.

O painel é a porta de entrada do funil de aquisição do SaaS e opera sempre no contexto de um único tenant (a barbearia do usuário logado), garantindo o isolamento de dados entre os assinantes.

## Tecnologias

| Item | Escolha | Observação |
|---|---|---|
| Framework | **Angular** | Single Page Application responsável pelo painel |
| Idioma | TypeScript | Tipagem estática e alinhada com a API .NET |
| Roteamento | Angular Router | Navegação com guarda de rotas por papel (`admin`) |
| Estado / HTTP | Services + RxJS (HttpClient) | Consumidor da API barbearia (`api-barber`) |
| Estilização | Definida pelo design system do projeto | Identidade visual "Navalha Dourada" (grafite + dourado) |

## Papéis e Permissões

A interface é exclusiva do papel **admin** (dono da barbearia). Demais papéis (barbeiro, cliente) não possuem acesso ao painel — a verificação do papel e do status do plano é feita também pelo middleware da API.

## Módulos e Telas

| Módulo | Descrição |
|---|---|
| Autenticação | Login, cadastro da barbearia (CPF/CNPJ do dono), recuperação de senha. O cadastro cria o tenant, o usuário admin e a assinatura recorrente na Asaas |
| Dashboard | Indicadores do dia/semana/mês: receita, agendamentos, clientes, serviços mais agendados |
| Plano e Faturas | Visualização do plano contratado, histórico de faturas (pagas, em aberto, vencidas), upgrade de plano e cancelamento |
| Escala | Cadastro dos dias e horários de trabalho de cada barbeiro (start/end) |
| Serviços | CRUD de serviços vinculados aos tipos de serviço (com nome, duração em minutos e valor) |
| Barbeiros | CRUD da equipe, com envio de credenciais de primeiro acesso por e-mail |
| Agendamentos | Visualização dos agendamentos da barbearia, com filtros |

## Integrações

O painel integra-se com a **API barbearia** (`api-barber`) para todas as operações de negócio e com o **Firebase Auth** para autenticação (e-mail/senha e recuperação de senha — o login Google não será implementado). O fluxo de assinatura (criação do customer e cobrança recorrente) é orquestrado pela API em conjunto com a **Asaas**.

## Regras Transversais

A requisição sempre carrega o JWT emitido pela API (contendo `userId`, `role`, `barbershopId` e `firebaseUid`), e o usuário logado visualiza apenas os dados da própria barbearia. O acesso permanece liberado enquanto o plano estiver ativo ou vencido há no máximo 5 dias.

## Documentação Relacionada

- [Documentação do SaaS — seções 7, 9 e 10](../docs/documentation.md)
- [Cartão "Requisitos/Tela do web"](https://trello.com/c/bAkTOl0m/3-requisitos-tela-do-web)
