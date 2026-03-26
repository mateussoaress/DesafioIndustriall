namespace PurchaseOrderApi.Application.DTOs.Requests;

/// <summary>
/// DTO de entrada para envio do pedido para aprovação.
/// </summary>
public class SubmitRequest
{
    /// <summary>ID do usuário elaborador que está enviando o pedido.</summary>
    public Guid UserId { get; set; }
}
