namespace PurchaseOrderApi.Domain.Enums;

/// <summary>
/// Tipos de ação registráveis no histórico do pedido de compras.
/// Cada ação representa um evento significativo no ciclo de vida do pedido (RN6).
/// </summary>
public enum HistoryAction
{
    /// <summary>Pedido criado pelo elaborador.</summary>
    Created = 0,

    /// <summary>Pedido enviado para aprovação (primeira vez ou após revisão).</summary>
    Submitted = 1,

    /// <summary>Pedido aprovado por um nível da cadeia de aprovação.</summary>
    Approved = 2,

    /// <summary>Revisão solicitada por um aprovador — pedido retorna ao elaborador.</summary>
    RevisionRequested = 3,

    /// <summary>Pedido reenviado pelo elaborador após ajustes de revisão.</summary>
    Resubmitted = 4,

    /// <summary>Pedido cancelado por um aprovador.</summary>
    Cancelled = 5
}
