# saas-barber

Repositório central do SaaS **APP Barbearia + Web (Versão 1)** — sistema de agendamentos para barbearias.

## Sobre o projeto

O SaaS é composto por três projetos que operam de forma integrada, mais a documentação que define escopo e requisitos da V1. O modelo de negócio é de assinatura (R$ 39,99/mês) via Asaas, com autenticação e notificações via Firebase.

## Estrutura do Repositório

| Pasta | Projeto | Descrição |
|---|---|---|
| [`api-barber/`](api-barber/) | Backend (.NET) | API REST, middleware de plano/auth, services, repositories e handlers externos (Asaas, Firebase). Ver `OVERVIEW.md` |
| [`ui-barber/`](ui-barber/) | Painel Web (Angular) | Admin do dono da barbearia: dashboard, plano e faturas, escala, serviços, barbeiros e agendamentos. Ver `OVERVIEW.md` |
| [`app-barber/`](app-barber/) | App Mobile (Flutter) | App único iOS/Android para clientes e barbeiros (com papel de admin para gestão de serviços e equipe). Ver `OVERVIEW.md` |
| [`docs/`](docs/) | Documentação | Documentação completa do SaaS (v1.3): escopo, requisitos, modelagem MongoDB, arquitetura da API, integração Asaas e landing page |

## Documentação

A documentação é a fonte de referência oficial do escopo e requisitos da V1, sincronizada a partir da página do Notion ([Documentação do SaaS — APP Barbearia + Web](https://app.notion.com/p/3b68d27657cf8123b25fedda9ddb28b5)), que cobre 13 seções: Visão Geral, Escopo, Stack Tecnológica, Requisitos do App e da Web, Modelagem do Banco de Dados, Arquitetura da API, Integração Asaas, Sugestões de Melhoria, Landing Page e Referência aos cartões do Trello.

O backlog de trabalho é gerenciado no quadro [SAAS - APP Barbearia + Web - VERSÃO 1](https://trello.com/b/nvlYcWDY/saas-app-barbearia-web-vers%C3%A3o-1) no Trello.
