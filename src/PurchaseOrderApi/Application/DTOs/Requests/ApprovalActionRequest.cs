using PurchaseOrderApi.Domain.Enums;

namespace PurchaseOrderApi.Application.DTOs.Requests;

/// <summary>
/// DTO de entrada para ações de aprovação (aprovar, solicitar revisão, cancelar).
/// </summary>
public class ApprovalActionRequest
{
    /// <summary>ID do usuário que está realizando a ação.</summary>
    public Guid UserId { get; set; }

    /// <summary>Nível de aprovação do usuário (Supplies, Manager, Director).</summary>
    public ApprovalLevel ApproverLevel { get; set; }

    /// <summary>Comentários ou motivo da ação (obrigatório para revisão e cancelamento).</summary>
    public string? Comments { get; set; }
}
