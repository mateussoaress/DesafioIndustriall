namespace PurchaseOrderApi.Domain.Enums;

/// <summary>
/// Representa o estado de cada etapa individual de aprovação.
/// Cada registro de Approval possui seu próprio status, independente do status do pedido.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>Aprovação pendente — aguardando ação do aprovador.</summary>
    Pending = 0,

    /// <summary>Aprovado pelo aprovador deste nível.</summary>
    Approved = 1,

    /// <summary>Rejeitado pelo aprovador deste nível.</summary>
    Rejected = 2,

    /// <summary>Revisão solicitada pelo aprovador — pedido retorna ao elaborador.</summary>
    RevisionRequested = 3
}
