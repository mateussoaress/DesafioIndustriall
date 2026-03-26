# Diagrama Físico de Banco de Dados — Pedido de Compras

```mermaid
erDiagram
    Users {
        uniqueidentifier Id PK "NOT NULL"
        nvarchar(150) Name "NOT NULL"
        int Profile "NOT NULL (0=Collaborator, 1=Supplies, 2=Manager, 3=Director)"
    }

    PurchaseOrders {
        uniqueidentifier Id PK "NOT NULL"
        datetime2 CreatedAt "NOT NULL"
        decimal(18_2) TotalValue "NOT NULL"
        int Status "NOT NULL (0=Draft, 1=AwaitingApproval, 2=UnderReview, 3=Approved, 4=Rejected, 5=Cancelled)"
        uniqueidentifier CreatorUserId FK "NOT NULL"
    }

    OrderItems {
        uniqueidentifier Id PK "NOT NULL"
        uniqueidentifier PurchaseOrderId FK "NOT NULL"
        nvarchar(300) ProductName "NOT NULL"
        int Quantity "NOT NULL"
        decimal(18_2) UnitPrice "NOT NULL"
    }

    Approvals {
        uniqueidentifier Id PK "NOT NULL"
        uniqueidentifier PurchaseOrderId FK "NOT NULL"
        int Level "NOT NULL (0=Supplies, 1=Manager, 2=Director)"
        int Status "NOT NULL (0=Pending, 1=Approved, 2=Rejected, 3=RevisionRequested)"
        uniqueidentifier ApproverUserId FK "NULL"
        datetime2 ApprovalDate "NULL"
        nvarchar(1000) Comments "NULL"
    }

    OrderHistories {
        uniqueidentifier Id PK "NOT NULL"
        uniqueidentifier PurchaseOrderId FK "NOT NULL"
        int Action "NOT NULL (0=Created, 1=Submitted, 2=Approved, 3=RevisionRequested, 4=Resubmitted, 5=Cancelled)"
        uniqueidentifier UserId FK "NOT NULL"
        datetime2 CreatedAt "NOT NULL"
        nvarchar(1000) Description "NOT NULL"
    }

    Users ||--o{ PurchaseOrders : "cria"
    Users ||--o{ Approvals : "aprova"
    Users ||--o{ OrderHistories : "realiza"
    PurchaseOrders ||--o{ OrderItems : "contém"
    PurchaseOrders ||--o{ Approvals : "possui"
    PurchaseOrders ||--o{ OrderHistories : "registra"
```

## Detalhes do Modelo Relacional

### Tabela: Users
| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| Id | uniqueidentifier | PK, NOT NULL | Identificador único do usuário |
| Name | nvarchar(150) | NOT NULL | Nome do usuário |
| Profile | int | NOT NULL | Perfil do usuário (enum UserProfile) |

### Tabela: PurchaseOrders
| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| Id | uniqueidentifier | PK, NOT NULL | Identificador único do pedido |
| CreatedAt | datetime2 | NOT NULL | Data/hora de criação |
| TotalValue | decimal(18,2) | NOT NULL | Valor total calculado (soma dos itens) |
| Status | int | NOT NULL, INDEX | Status do pedido (enum OrderStatus) |
| CreatorUserId | uniqueidentifier | FK, NOT NULL, INDEX | Referência ao usuário criador |

### Tabela: OrderItems
| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| Id | uniqueidentifier | PK, NOT NULL | Identificador único do item |
| PurchaseOrderId | uniqueidentifier | FK, NOT NULL, INDEX | Referência ao pedido |
| ProductName | nvarchar(300) | NOT NULL | Nome do produto |
| Quantity | int | NOT NULL | Quantidade |
| UnitPrice | decimal(18,2) | NOT NULL | Preço unitário |

### Tabela: Approvals
| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| Id | uniqueidentifier | PK, NOT NULL | Identificador único da aprovação |
| PurchaseOrderId | uniqueidentifier | FK, NOT NULL, INDEX | Referência ao pedido |
| Level | int | NOT NULL | Nível de aprovação (enum ApprovalLevel) |
| Status | int | NOT NULL | Status da aprovação (enum ApprovalStatus) |
| ApproverUserId | uniqueidentifier | FK, NULL | Referência ao usuário aprovador |
| ApprovalDate | datetime2 | NULL | Data/hora da aprovação |
| Comments | nvarchar(1000) | NULL | Comentários do aprovador |

### Tabela: OrderHistories
| Coluna | Tipo | Restrições | Descrição |
|--------|------|-----------|-----------|
| Id | uniqueidentifier | PK, NOT NULL | Identificador único do registro |
| PurchaseOrderId | uniqueidentifier | FK, NOT NULL, INDEX | Referência ao pedido |
| Action | int | NOT NULL, INDEX | Tipo da ação (enum HistoryAction) |
| UserId | uniqueidentifier | FK, NOT NULL | Referência ao usuário que realizou a ação |
| CreatedAt | datetime2 | NOT NULL | Data/hora da ação |
| Description | nvarchar(1000) | NOT NULL | Descrição da ação |

### Índices
- `IX_PurchaseOrders_Status` — consultas por status do pedido
- `IX_PurchaseOrders_CreatorUserId` — consultas por elaborador
- `IX_OrderItems_PurchaseOrderId` — junção com pedido
- `IX_Approvals_PurchaseOrderId` — junção com pedido
- `IX_Approvals_ApproverUserId` — consultas por aprovador
- `IX_OrderHistories_PurchaseOrderId` — junção com pedido
- `IX_OrderHistories_UserId` — consultas por usuário no histórico
- `IX_OrderHistories_Action` — filtros por tipo de ação
- `IX_Users_Name` — consultas por nome
- `IX_Users_Profile` — consultas por perfil

### Relacionamentos
- **Users → PurchaseOrders**: 1:N com RESTRICT DELETE (não pode excluir usuário com pedidos)
- **Users → Approvals**: 1:N com RESTRICT DELETE (opcional — ApproverUserId é nullable)
- **Users → OrderHistories**: 1:N com RESTRICT DELETE
- **PurchaseOrders → OrderItems**: 1:N com CASCADE DELETE
- **PurchaseOrders → Approvals**: 1:N com CASCADE DELETE
- **PurchaseOrders → OrderHistories**: 1:N com CASCADE DELETE
