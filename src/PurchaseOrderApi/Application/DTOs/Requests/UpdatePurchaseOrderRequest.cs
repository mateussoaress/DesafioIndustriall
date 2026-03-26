namespace PurchaseOrderApi.Application.DTOs.Requests;

/// <summary>
/// DTO de entrada para atualização de um pedido de compra em rascunho ou revisão.
/// </summary>
public class UpdatePurchaseOrderRequest
{
    /// <summary>Lista atualizada de itens do pedido. Deve conter ao menos 1 item (RN1).</summary>
    public List<OrderItemRequest> Items { get; set; } = new();
}
