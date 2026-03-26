using PurchaseOrderApi.Domain.Enums;

namespace PurchaseOrderApi.Domain.Entities;

/// <summary>
/// Registra todas as ações do ciclo de vida do pedido para rastreabilidade.
/// Contém validação simples de formato no construtor.
/// Relacionamentos: pertence a um PurchaseOrder e a um User.
/// </summary>
public class OrderHistory
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public HistoryAction Action { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Navegação para o pedido de compra relacionado.</summary>
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    /// <summary>Navegação para o usuário que realizou a ação.</summary>
    public User User { get; set; } = null!;

    private OrderHistory() { }

    public OrderHistory(HistoryAction action, Guid userId, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição da ação é obrigatória.", nameof(description));

        Id = Guid.NewGuid();
        Action = action;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        Description = description.Trim();
    }
}
