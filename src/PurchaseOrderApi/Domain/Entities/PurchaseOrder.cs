using PurchaseOrderApi.Domain.Enums;

namespace PurchaseOrderApi.Domain.Entities;

/// <summary>
/// Entidade que representa um pedido de compra.
/// Contém apenas dados e validações simples — regras de negócio ficam no Service.
/// Relacionamentos: pertence a um User (criador), contém 1+ OrderItems,
/// possui 0+ Approvals e 0+ OrderHistories.
/// </summary>
public class PurchaseOrder
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalValue { get; set; }
    public OrderStatus Status { get; set; }
    public Guid CreatorUserId { get; set; }

    /// <summary>Navegação para o usuário criador do pedido.</summary>
    public User CreatorUser { get; set; } = null!;

    /// <summary>Itens do pedido de compra.</summary>
    public List<OrderItem> Items { get; set; } = new();

    /// <summary>Etapas de aprovação do pedido.</summary>
    public List<Approval> Approvals { get; set; } = new();

    /// <summary>Histórico de ações do pedido (rastreabilidade).</summary>
    public List<OrderHistory> History { get; set; } = new();

    private PurchaseOrder() { }

    public PurchaseOrder(Guid creatorUserId, List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("O pedido deve conter pelo menos 1 item.", nameof(items));

        Id = Guid.NewGuid();
        CreatorUserId = creatorUserId;
        Status = OrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        Items = items;
        TotalValue = items.Sum(i => i.TotalPrice);
    }
}
