namespace PurchaseOrderApi.Application.DTOs.Requests;

/// <summary>
/// DTO de entrada para criação de um novo pedido de compra.
/// </summary>
public class CreatePurchaseOrderRequest
{
    /// <summary>ID do usuário que está criando o pedido.</summary>
    public Guid CreatorUserId { get; set; }

    /// <summary>Lista de itens do pedido. Deve conter ao menos 1 item (RN1).</summary>
    public List<OrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// DTO de entrada para um item do pedido de compra.
/// </summary>
public class OrderItemRequest
{
    /// <summary>Nome do produto.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Quantidade do item.</summary>
    public int Quantity { get; set; }

    /// <summary>Preço unitário do item.</summary>
    public decimal UnitPrice { get; set; }
}
