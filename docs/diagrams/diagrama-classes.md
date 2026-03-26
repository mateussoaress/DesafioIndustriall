# Diagrama de Classes — Pedido de Compras

```mermaid
classDiagram
    class PurchaseOrder {
        +Guid Id
        +DateTime CreatedAt
        +decimal TotalValue
        +OrderStatus Status
        +Guid CreatorUserId
        +User CreatorUser
        +List~OrderItem~ Items
        +List~Approval~ Approvals
        +List~OrderHistory~ History
    }

    class OrderItem {
        +Guid Id
        +Guid PurchaseOrderId
        +string ProductName
        +int Quantity
        +decimal UnitPrice
        +decimal TotalPrice
    }

    class Approval {
        +Guid Id
        +Guid PurchaseOrderId
        +ApprovalLevel Level
        +ApprovalStatus Status
        +Guid? ApproverUserId
        +DateTime? ApprovalDate
        +string? Comments
        +User? ApproverUser
    }

    class OrderHistory {
        +Guid Id
        +Guid PurchaseOrderId
        +HistoryAction Action
        +Guid UserId
        +DateTime CreatedAt
        +string Description
        +User User
    }

    class User {
        +Guid Id
        +string Name
        +UserProfile Profile
        +List~PurchaseOrder~ CreatedOrders
        +List~Approval~ Approvals
        +List~OrderHistory~ OrderHistories
    }

    class OrderStatus {
        <<enumeration>>
        Draft
        AwaitingApproval
        UnderReview
        Approved
        Rejected
        Cancelled
    }

    class ApprovalLevel {
        <<enumeration>>
        Supplies
        Manager
        Director
    }

    class ApprovalStatus {
        <<enumeration>>
        Pending
        Approved
        Rejected
        RevisionRequested
    }

    class HistoryAction {
        <<enumeration>>
        Created
        Submitted
        Approved
        RevisionRequested
        Resubmitted
        Cancelled
    }

    class UserProfile {
        <<enumeration>>
        Collaborator
        Supplies
        Manager
        Director
    }

    class IPurchaseOrderRepository {
        <<interface>>
        +GetByIdAsync(id: Guid) Task~PurchaseOrder?~
        +GetAllAsync() Task~IEnumerable~
        +AddAsync(order: PurchaseOrder) Task
        +UpdateAsync(order: PurchaseOrder) Task
    }

    class IUserRepository {
        <<interface>>
        +GetByIdAsync(id: Guid) Task~User?~
        +GetAllAsync() Task~IEnumerable~
        +AddAsync(user: User) Task
    }

    class IPurchaseOrderService {
        <<interface>>
        +CreateAsync(request) Task~Response~
        +GetByIdAsync(id: Guid) Task~Response~
        +GetAllAsync() Task~IEnumerable~
        +UpdateAsync(id: Guid, request) Task~Response~
        +SubmitAsync(id: Guid, request) Task~Response~
        +ApproveAsync(id: Guid, request) Task~Response~
        +RequestRevisionAsync(id: Guid, request) Task~Response~
        +ResubmitAsync(id: Guid, request) Task~Response~
        +CancelAsync(id: Guid, request) Task~Response~
    }

    class IUserService {
        <<interface>>
        +GetAllAsync() Task~IEnumerable~
        +GetByIdAsync(id: Guid) Task~Response~
    }

    class PurchaseOrderService {
        -IPurchaseOrderRepository _repository
        -ILogger _logger
        +CreateAsync(request) Task~Response~
        +GetByIdAsync(id: Guid) Task~Response~
        +GetAllAsync() Task~IEnumerable~
        +UpdateAsync(id: Guid, request) Task~Response~
        +SubmitAsync(id: Guid, request) Task~Response~
        +ApproveAsync(id: Guid, request) Task~Response~
        +RequestRevisionAsync(id: Guid, request) Task~Response~
        +ResubmitAsync(id: Guid, request) Task~Response~
        +CancelAsync(id: Guid, request) Task~Response~
        +GetRequiredApprovalLevel(totalValue: decimal)$ ApprovalLevel
        -CreateApprovalChain(order) void
        -GetCurrentPendingApproval(order) Approval
        -ValidateTransition(order, status, action) void
        -ValidateCreator(order, userId, msg) void
    }

    class UserService {
        -IUserRepository _repository
        +GetAllAsync() Task~IEnumerable~
        +GetByIdAsync(id: Guid) Task~Response~
    }

    class PurchaseOrderRepository {
        -AppDbContext _context
    }

    class UserRepository {
        -AppDbContext _context
    }

    PurchaseOrder "1" --> "*" OrderItem : contém
    PurchaseOrder "1" --> "*" Approval : possui
    PurchaseOrder "1" --> "*" OrderHistory : registra
    PurchaseOrder "*" --> "1" User : criado por
    Approval "*" --> "0..1" User : aprovado por
    OrderHistory "*" --> "1" User : realizado por
    PurchaseOrder --> OrderStatus : status
    Approval --> ApprovalLevel : nível
    Approval --> ApprovalStatus : status
    OrderHistory --> HistoryAction : tipo de ação
    User --> UserProfile : perfil
    IPurchaseOrderService <|.. PurchaseOrderService : implementa
    IUserService <|.. UserService : implementa
    IPurchaseOrderRepository <|.. PurchaseOrderRepository : implementa
    IUserRepository <|.. UserRepository : implementa
    PurchaseOrderService --> IPurchaseOrderRepository : usa
    UserService --> IUserRepository : usa
```

## Decisões de Modelagem

- **Anemic Domain Model** — as entidades apenas carregam dados (propriedades públicas, sem métodos de negócio)
- **Toda a lógica de negócio está no `PurchaseOrderService`** — regras de aprovação, transição de status, cadeia de alçadas (RN1 a RN8)
- **Fluxo Controller → Service → Repository** — os controllers nunca acessam repositórios diretamente
- **OrderItem** é uma entidade dependente — só existe no contexto de um pedido
- **Approval** representa cada etapa individual de aprovação com seu próprio status
- **OrderHistory** é imutável após criação — garante rastreabilidade (RN6)
- **User** possui perfil que define sua autoridade no fluxo de aprovação
- **Enums** representam estados finitos e bem definidos do domínio
- **Interfaces** (Repository e Service) garantem desacoplamento e testabilidade
