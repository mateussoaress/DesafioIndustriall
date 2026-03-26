namespace PurchaseOrderApi.Application.DTOs.Requests;

/// <summary>
/// DTO de entrada para reenvio de um pedido após revisão.
/// Permite atualizar os itens durante o reenvio.
/// </summary>
public class ResubmitRequest
{
    /// <summary>ID do usuário que está reenviando o pedido.</summary>
    public Guid UserId { get; set; }

    /// <summary>Lista atualizada de itens (opcional — se vazio, mantém os itens atuais).</summary>
    public List<OrderItemRequest>? Items { get; set; }
}
