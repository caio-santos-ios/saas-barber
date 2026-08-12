# app-barber — Aplicativo Mobile (Flutter)

## Visão Geral

O **app-barber** é o aplicativo mobile único do SaaS APP Barbearia + Web (Versão 1), desenvolvido em **Flutter** para atender iOS e Android com uma única codebase. O mesmo app serve a dois públicos distintos, com experiências adaptadas pelo papel do usuário autenticado:

| Público | Papel | O que usa no app |
|---|---|---|
| **Clientes da barbearia** | `customer` | Agendamentos, perfil e notificações |
| **Barbeiros** | `barber` / `admin` (barbeiro admin) | Agenda pessoal, gestão de status, e para o admin: serviços e equipe |

## Tecnologias

| Item | Escolha | Observação |
|---|---|---|
| Framework | **Flutter** | App único para iOS e Android |
| Idioma | Dart | Tipagem estática |
| Autenticação | Firebase Auth | Login Google e e-mail/senha; recuperação de senha por e-mail |
| Notificações | Firebase Cloud Messaging | Push de confirmação, status e avisos |
| Comunicação | API REST (`api-barber`) | Todos os dados vêm do backend, nunca do armazenamento local |

## Funcionalidades por Papel

### Cliente (`customer`)

O cliente navega por Home, Meus Agendamentos e Perfil. Na **Home** vê o card de boas-vindas, o próximo agendamento e quatro indicadores (total, cancelados, fazendo e feitos). Em **Meus Agendamentos** lista, filtra, adiciona, edita, cancela e exclui agendamentos — sempre buscando e exibindo apenas os seus próprios registros. O **novo agendamento** segue o fluxo: seletor com os **barbeiros** e os **serviços** disponíveis → **data disponível daquele barbeiro** → **horário disponível daquele barbeiro** → confirmação com resumo; o único campo opcional é a **observação**. Ao confirmar, o agendamento recebe o status `marcado`. **Regra de edição:** o botão de editar só aparece **até 24 horas antes** do horário agendado; o cancelamento é permitido para agendamentos não finalizados.

### Barbeiro (`barber`)

O barbeiro **sempre usa o ID do usuário logado** para exibir e buscar informações de agendamentos. Ele visualiza seus **agendamentos do dia**, marca cada um como `fazendo` e depois `feito` (status que dispara notificação ao cliente e ao admin). Também consulta seu perfil e pode visualizar sua agenda.

### Barbeiro Admin (`admin` no app)

Além das funções do barbeiro, o barbeiro admin gerencia os **serviços** (CRUD: nome, tipo, valor, comissão, fotos, duração) e os **barbeiros** da barbearia (CRUD), incluindo o envio das credenciais de primeiro acesso dos novos barbeiros por e-mail. Ele não acessa o painel web — o dashboard completo pertence ao dono (módulo `ui-barber`).

## Regras Transversais

O app opera com **modo escuro e claro**. Todo acesso é contextualizado pelo usuário logado (JWT emitido pela API com `userId`, `role` e `barbershopId`), e o usuário visualiza apenas dados da sua barbearia e da sua agenda. O app exibe conteúdo do tenant apenas enquanto o plano estiver ativo (regra aplicada pelo middleware da API). As notificações push alimentam o ícone do sininho na Home.

## Estrutura de Telas (protótipo de referência)

O protótipo navegável construído para validação das telas (hub com os quatro papéis) serve como referência visual: https://barberapp-kw36m33k.manus.space (hub → App Cliente / App Barbeiro / App Barbeiro Admin).

## Documentação Relacionada

- [Documentação do SaaS — seções 4, 5, 6 e 8](../docs/documentation.md)
- [Cartão "Requisitos/Telas do App"](https://trello.com/c/y4qjG0Jr/2-requisitos-telas-do-app)
- [Cartão "Collection MongoDB"](https://trello.com/c/1GELmyiW/4-collection-mongodb)
