using PurchaseOrderApi.Domain.Enums;

namespace PurchaseOrderApi.Domain.Entities;

/// <summary>
/// Representa cada etapa individual de aprovação de um pedido de compra.
/// Contém apenas dados — regras de negócio ficam no Service.
/// Relacionamentos: pertence a um PurchaseOrder e a um User (aprovador).
/// </summary>
public class Approval
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public ApprovalLevel Level { get; set; }
    public ApprovalStatus Status { get; set; }
    public Guid? ApproverUserId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Comments { get; set; }

    /// <summary>Navegação para o pedido de compra.</summary>
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    /// <summary>Navegação para o usuário aprovador.</summary>
    public User? ApproverUser { get; set; }

    private Approval() { }

    public Approval(ApprovalLevel level)
    {
        Id = Guid.NewGuid();
        Level = level;
        Status = ApprovalStatus.Pending;
    }
}
