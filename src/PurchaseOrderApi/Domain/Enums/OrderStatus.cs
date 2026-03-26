namespace PurchaseOrderApi.Domain.Enums;

/// <summary>
/// Representa os possíveis estados de um pedido de compra durante seu ciclo de vida.
/// </summary>
public enum OrderStatus
{
    /// <summary>Pedido criado, ainda não enviado para aprovação.</summary>
    Draft = 0,

    /// <summary>Pedido aguardando aprovação do próximo nível da cadeia.</summary>
    AwaitingApproval = 1,

    /// <summary>Pedido devolvido ao elaborador para revisão/ajustes.</summary>
    UnderReview = 2,

    /// <summary>Pedido aprovado — todas as aprovações da alçada foram obtidas.</summary>
    Approved = 3,

    /// <summary>Pedido rejeitado por um aprovador.</summary>
    Rejected = 4,

    /// <summary>Pedido cancelado por um aprovador.</summary>
    Cancelled = 5
}
