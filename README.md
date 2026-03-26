# Purchase Order API — Desafio Tecnico IndustriALL

API REST em C# (.NET 8) para gerenciamento do processo de pedido de compras com fluxo de aprovacao hierarquica.

---

## Sumario

- [Arquitetura](#arquitetura)
- [Modelagem de Dominio](#modelagem-de-dominio)
- [Decisoes de Modelagem](#decisoes-de-modelagem)
- [Pre-requisitos](#pre-requisitos)
- [Como Executar](#como-executar)
- [Migrations](#migrations)
- [Endpoints da API](#endpoints-da-api)
- [Exemplos de Payload](#exemplos-de-payload)
- [Como Testar](#como-testar)
- [Testes Unitarios](#testes-unitarios)
- [Diagramas](#diagramas)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Observacoes Importantes](#observacoes-importantes)

---

## Arquitetura

A solucao utiliza uma **arquitetura em camadas** dentro de um unico projeto, priorizando simplicidade e clareza sem sacrificar separacao de responsabilidades:

```
src/PurchaseOrderApi/
├── Controllers/        → Endpoints REST (controllers finos)
├── Application/        → DTOs, Validadores, Services (regras de negocio)
├── Domain/             → Entidades anemicas, Enums, Interfaces
├── Infrastructure/     → DbContext, Configurations EF Core, Repositorios
└── Middleware/          → Tratamento global de excecoes
```

**Fluxo da requisicao:**
```
Controller → Service → Repository → Banco de Dados
```

**Por que essa estrutura?**
- Para o escopo do desafio, multiplos projetos (.sln com N .csproj) adicionariam complexidade sem beneficio real. Em um projeto maior, cada camada seria um .csproj separado
- A separacao por pastas mantem o codigo organizado e as responsabilidades claras
- As entidades de dominio sao **anemicas** — contem apenas dados e validacoes simples de formato
- As **regras de negocio ficam nos Services** da camada Application, seguindo o padrao mais adotado em projetos .NET corporativos
- Os controllers sao finos — recebem a requisicao, chamam o Service e retornam o resultado

---

## Modelagem de Dominio

### Entidades

| Entidade | Responsabilidade |
|----------|-----------------|
| **PurchaseOrder** | Representa o pedido de compra. Contem dados do pedido e relacionamentos |
| **OrderItem** | Item do pedido. Contem nome do produto, quantidade e preco unitario |
| **Approval** | Cada etapa individual de aprovacao com seu proprio status |
| **OrderHistory** | Registro de cada acao no ciclo de vida do pedido (rastreabilidade) |
| **User** | Usuario do sistema com perfil que define sua autoridade no fluxo |

### Enums

| Enum | Valores |
|------|---------|
| **OrderStatus** | Draft, AwaitingApproval, UnderReview, Approved, Rejected, Cancelled |
| **ApprovalLevel** | Supplies (0), Manager (1), Director (2) |
| **ApprovalStatus** | Pending, Approved, Rejected, RevisionRequested |
| **HistoryAction** | Created, Submitted, Approved, RevisionRequested, Resubmitted, Cancelled |
| **UserProfile** | Collaborator, Supplies, Manager, Director |

### Fluxo de Estados

```
Draft → AwaitingApproval → Approved
              ↓ ↘
        UnderReview  Cancelled
              ↓
        (Resubmit) → AwaitingApproval
```

---

## Decisoes de Modelagem

### Por que Anemic Domain Model?
As entidades contem apenas propriedades e validacoes simples de formato (campo obrigatorio, valor positivo). Toda regra de negocio reside na camada de Application (Services). Isso garante:
- Entidades limpas e previsiveis — sao apenas representacoes dos dados
- Services testaveis — toda logica fica concentrada em classes que recebem dependencias por injecao
- Separacao clara de responsabilidades — regra de negocio esta nos Services, nao espalhada entre entidades e controllers
- Manutencao simplificada — mudar uma regra de negocio significa mexer em um Service

### Por que uma tabela Approvals separada?
Cada nivel de aprovacao e uma instancia de `Approval` com seu proprio status (`Pending`, `Approved`, `Rejected`, `RevisionRequested`). Isso permite rastrear individualmente quem aprovou, quando e com quais comentarios.

### Por que FluentValidation?
Separa validacao de formato/entrada (DTOs) da validacao de regras de negocio (Services). Erros de validacao retornam 400; erros de negocio retornam 422.

### Por que TotalValue e calculado e persistido?
O valor total e calculado pela soma de `Quantity x UnitPrice` de cada item e armazenado na coluna `TotalValue`. E recalculado automaticamente sempre que os itens sao alterados, garantindo consistencia.

---

## Pre-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, Developer ou Azure SQL)

---

## Como Executar

### 1. Configurar a connection string

Edite o arquivo `src/PurchaseOrderApi/appsettings.json` com os dados do seu SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=PurchaseOrderDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Para Azure SQL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu-servidor.database.windows.net;Database=PurchaseOrderDb;User Id=seu-usuario;Password=sua-senha;Encrypt=True;TrustServerCertificate=False;"
  }
}
```

### 2. Executar a API

```bash
dotnet run --project src/PurchaseOrderApi
```

A API estara disponivel em:
- **Swagger UI**: http://localhost:5009
- **API**: http://localhost:5009/api/v1/purchaseorders

> Em ambiente de desenvolvimento, as **migrations sao aplicadas automaticamente** e os **usuarios de teste sao criados** ao iniciar a aplicacao.

### Usuarios de Teste (Seed)

| Nome | Perfil | Autoridade |
|------|--------|-----------|
| Joao Silva | Collaborator | Criar e editar pedidos |
| Maria Souza | Supplies | Aprovar (1o nivel) |
| Carlos Oliveira | Manager | Aprovar (2o nivel) |
| Ana Costa | Director | Aprovar (3o nivel) |

---

## Migrations

### Aplicar migrations manualmente

```bash
cd src/PurchaseOrderApi
dotnet ef database update
```

### Criar nova migration

```bash
cd src/PurchaseOrderApi
dotnet ef migrations add NomeDaMigration --output-dir Infrastructure/Data/Migrations
```

---

## Endpoints da API

### PurchaseOrders

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| `POST` | `/api/v1/purchaseorders` | Criar novo pedido |
| `GET` | `/api/v1/purchaseorders` | Listar todos os pedidos |
| `GET` | `/api/v1/purchaseorders/{id}` | Obter pedido por ID |
| `PUT` | `/api/v1/purchaseorders/{id}` | Atualizar pedido (Draft/UnderReview) |
| `POST` | `/api/v1/purchaseorders/{id}/submit` | Enviar para aprovacao |
| `POST` | `/api/v1/purchaseorders/{id}/approve` | Aprovar no nivel atual |
| `POST` | `/api/v1/purchaseorders/{id}/request-revision` | Solicitar revisao |
| `POST` | `/api/v1/purchaseorders/{id}/resubmit` | Reenviar apos revisao |
| `POST` | `/api/v1/purchaseorders/{id}/cancel` | Cancelar pedido |

### Users

| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| `GET` | `/api/v1/users` | Listar todos os usuarios |
| `GET` | `/api/v1/users/{id}` | Obter usuario por ID |

---

## Exemplos de Payload

### Criar Pedido

```json
POST /api/v1/purchaseorders

{
  "creatorUserId": "guid-do-joao-silva",
  "items": [
    {
      "productName": "Caneta esferografica",
      "quantity": 10,
      "unitPrice": 2.50
    },
    {
      "productName": "Caderno universitario",
      "quantity": 5,
      "unitPrice": 12.00
    }
  ]
}
```

### Enviar para Aprovacao

```json
POST /api/v1/purchaseorders/{id}/submit

{
  "userId": "guid-do-joao-silva"
}
```

### Aprovar

```json
POST /api/v1/purchaseorders/{id}/approve

{
  "userId": "guid-da-maria-souza",
  "approverLevel": "Supplies",
  "comments": "Itens verificados e aprovados."
}
```

> **ApproverLevel** aceita: `"Supplies"`, `"Manager"`, `"Director"` (ou `0`, `1`, `2`)

### Solicitar Revisao

```json
POST /api/v1/purchaseorders/{id}/request-revision

{
  "userId": "guid-da-maria-souza",
  "approverLevel": "Supplies",
  "comments": "Revisar quantidade do item 1."
}
```

### Reenviar apos Revisao

```json
POST /api/v1/purchaseorders/{id}/resubmit

{
  "userId": "guid-do-joao-silva",
  "items": [
    {
      "productName": "Caneta esferografica",
      "quantity": 5,
      "unitPrice": 2.50
    }
  ]
}
```

### Cancelar

```json
POST /api/v1/purchaseorders/{id}/cancel

{
  "userId": "guid-da-maria-souza",
  "approverLevel": "Supplies",
  "comments": "Pedido duplicado."
}
```

---

## Como Testar

### Via Swagger

Acesse http://localhost:5009 no navegador. A interface do Swagger permite testar todos os endpoints diretamente.

### Via Postman/Insomnia

Importe a colecao disponivel em:
```
PurchaseOrderApi.postman_collection.json
```

### Fluxo de Teste Sugerido

**Cenario 1 — Aprovacao simples (ate R$ 100):**
1. `GET /api/v1/users` → copiar IDs dos usuarios
2. `POST /api/v1/purchaseorders` → criar pedido (ex: 10 canetas x R$5 = R$50)
3. `POST /api/v1/purchaseorders/{id}/submit` → enviar para aprovacao
4. `POST /api/v1/purchaseorders/{id}/approve` → aprovar como Supplies → status: **Approved**

**Cenario 2 — Aprovacao completa (> R$ 1.000):**
1. Criar pedido (ex: 2 notebooks x R$2500 = R$5000)
2. Submit → Approve Supplies → Approve Manager → Approve Director → **Approved**

**Cenario 3 — Revisao e reenvio:**
1. Criar pedido → enviar para aprovacao
2. Solicitar revisao (request-revision)
3. Reenviar (resubmit) → fluxo reinicia do Supplies

**Cenario 4 — Cancelamento:**
1. Criar pedido → enviar para aprovacao
2. Cancelar (cancel) → status: **Cancelled**

---

## Testes Unitarios

```bash
dotnet test --verbosity normal
```

Os testes cobrem todas as regras de negocio criticas:

| Regra | Testes |
|-------|--------|
| RN1 — Minimo de 1 item | Criacao sem itens, com itens nulos |
| RN2 — Calculo do total | Multiplos itens, quantidade x preco |
| RN3 — Alcada de aprovacao | Boundary values (100, 101, 1000, 1001) |
| RN4 — Aprovacao sequencial | Avanco entre niveis, aprovacao fora de ordem |
| RN5 — Revisao e reinicio | Solicitacao de revisao, reenvio, reinicio da cadeia |
| RN6 — Historico | Registro de todas as acoes com dados corretos |
| RN7 — Aprovacao final | Aprovar em cada alcada (Supplies, Manager, Director) |
| RN8 — Cancelamento | Cancelar por qualquer nivel, validacoes |

**Total: 43 testes unitarios** (10 testes de entidade + 33 testes de Service com Moq)

---

## Diagramas

Os diagramas estao em formato **Mermaid** (versionaveis no repositorio):

| Diagrama | Arquivo |
|----------|---------|
| Diagrama de Atividades | [docs/diagrams/diagrama-atividades.md](docs/diagrams/diagrama-atividades.md) |
| Diagrama de Classes | [docs/diagrams/diagrama-classes.md](docs/diagrams/diagrama-classes.md) |
| Diagrama Fisico de Banco | [docs/diagrams/diagrama-banco.md](docs/diagrams/diagrama-banco.md) |

> Para visualizar os diagramas Mermaid, utilize o GitHub, VS Code com extensao Mermaid, ou [mermaid.live](https://mermaid.live).

---

## Estrutura do Projeto

```
DesafioIndustriall/
├── src/
│   └── PurchaseOrderApi/
│       ├── Controllers/
│       │   ├── PurchaseOrdersController.cs
│       │   └── UsersController.cs
│       ├── Application/
│       │   ├── DTOs/
│       │   │   ├── Requests/
│       │   │   │   ├── CreatePurchaseOrderRequest.cs
│       │   │   │   ├── UpdatePurchaseOrderRequest.cs
│       │   │   │   ├── ApprovalActionRequest.cs
│       │   │   │   ├── SubmitRequest.cs
│       │   │   │   └── ResubmitRequest.cs
│       │   │   └── Responses/
│       │   │       ├── PurchaseOrderResponse.cs
│       │   │       ├── UserResponse.cs
│       │   │       └── ErrorResponse.cs
│       │   ├── Services/
│       │   │   ├── IPurchaseOrderService.cs
│       │   │   ├── PurchaseOrderService.cs
│       │   │   ├── IUserService.cs
│       │   │   └── UserService.cs
│       │   └── Validators/
│       │       ├── CreatePurchaseOrderValidator.cs
│       │       ├── UpdatePurchaseOrderValidator.cs
│       │       ├── ApprovalActionValidator.cs
│       │       ├── SubmitRequestValidator.cs
│       │       └── ResubmitRequestValidator.cs
│       ├── Domain/
│       │   ├── Entities/
│       │   │   ├── PurchaseOrder.cs
│       │   │   ├── OrderItem.cs
│       │   │   ├── Approval.cs
│       │   │   ├── OrderHistory.cs
│       │   │   └── User.cs
│       │   ├── Enums/
│       │   │   ├── OrderStatus.cs
│       │   │   ├── ApprovalLevel.cs
│       │   │   ├── ApprovalStatus.cs
│       │   │   ├── HistoryAction.cs
│       │   │   └── UserProfile.cs
│       │   └── Interfaces/
│       │       ├── IPurchaseOrderRepository.cs
│       │       └── IUserRepository.cs
│       ├── Infrastructure/
│       │   ├── Data/
│       │   │   ├── AppDbContext.cs
│       │   │   ├── Configurations/
│       │   │   │   ├── PurchaseOrderConfiguration.cs
│       │   │   │   ├── OrderItemConfiguration.cs
│       │   │   │   ├── ApprovalConfiguration.cs
│       │   │   │   ├── OrderHistoryConfiguration.cs
│       │   │   │   └── UserConfiguration.cs
│       │   │   └── Migrations/
│       │   └── Repositories/
│       │       ├── PurchaseOrderRepository.cs
│       │       └── UserRepository.cs
│       ├── Middleware/
│       │   └── GlobalExceptionMiddleware.cs
│       └── Program.cs
├── tests/
│   └── PurchaseOrderApi.Tests/
│       ├── Domain/
│       │   ├── PurchaseOrderTests.cs
│       │   └── PurchaseOrderItemTests.cs
│       └── Application/
│           └── PurchaseOrderServiceTests.cs
├── docs/
│   └── diagrams/
│       ├── diagrama-atividades.md
│       ├── diagrama-classes.md
│       └── diagrama-banco.md
├── PurchaseOrderApi.postman_collection.json
└── README.md
```

---

## Observacoes Importantes

- **Nomes em ingles**: todas as classes, metodos, propriedades e variaveis seguem a convencao do desafio
- **Comentarios em portugues**: todas as docstrings e comentarios explicam a finalidade em portugues
- **Usuarios via seed**: os usuarios sao criados automaticamente via seed no startup. A API identifica usuarios por `userId` (Guid)
- **Migrations automaticas**: em ambiente de desenvolvimento, as migrations sao aplicadas automaticamente ao iniciar a aplicacao
- **Swagger na raiz**: a documentacao interativa esta disponivel na URL raiz da aplicacao (http://localhost:5009)
- **Tratamento de erros**: erros sao categorizados (400 = validacao, 404 = nao encontrado, 422 = regra de negocio, 500 = erro interno) com mensagens padronizadas
- **Compativel com Azure SQL**: basta alterar a connection string para usar Azure SQL

---

## Tecnologias Utilizadas

- .NET 8.0
- Entity Framework Core 8.0
- SQL Server (LocalDB / Azure SQL)
- FluentValidation
- Swashbuckle (Swagger/OpenAPI)
- xUnit + Moq (testes unitarios)
