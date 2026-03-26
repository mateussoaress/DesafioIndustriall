namespace PurchaseOrderApi.Domain.Entities;

/// <summary>
/// Representa cada item dentro de um pedido de compra.
/// Contém validações simples de formato no construtor.
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Valor total do item: quantidade x preço unitário.
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;

    /// <summary>Navegação para o pedido de compra ao qual pertence.</summary>
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    private OrderItem() { }

    public OrderItem(string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("O nome do produto é obrigatório.", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantity));
        if (unitPrice <= 0)
            throw new ArgumentException("O preço unitário deve ser maior que zero.", nameof(unitPrice));

        Id = Guid.NewGuid();
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
