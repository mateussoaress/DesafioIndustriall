using PurchaseOrderApi.Application.DTOs.Requests;
using PurchaseOrderApi.Application.DTOs.Responses;

namespace PurchaseOrderApi.Application.Services;

/// <summary>
/// Contrato do serviço de aplicação para pedidos de compra.
/// Define as operações disponíveis no fluxo de pedido de compras.
/// </summary>
public interface IPurchaseOrderService
{
    /// <summary>Cria um novo pedido de compra.</summary>
    Task<PurchaseOrderResponse> CreateAsync(CreatePurchaseOrderRequest request);

    /// <summary>Obtém um pedido de compra pelo identificador.</summary>
    Task<PurchaseOrderResponse> GetByIdAsync(Guid id);

    /// <summary>Obtém todos os pedidos de compra.</summary>
    Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync();

    /// <summary>Atualiza os dados de um pedido em rascunho ou revisão.</summary>
    Task<PurchaseOrderResponse> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request);

    /// <summary>Envia o pedido para o fluxo de aprovação.</summary>
    Task<PurchaseOrderResponse> SubmitAsync(Guid id, SubmitRequest request);

    /// <summary>Aprova o pedido no nível de aprovação atual.</summary>
    Task<PurchaseOrderResponse> ApproveAsync(Guid id, ApprovalActionRequest request);

    /// <summary>Solicita revisão do pedido, devolvendo ao elaborador.</summary>
    Task<PurchaseOrderResponse> RequestRevisionAsync(Guid id, ApprovalActionRequest request);

    /// <summary>Reenvia o pedido após revisão, reiniciando a cadeia de aprovação.</summary>
    Task<PurchaseOrderResponse> ResubmitAsync(Guid id, ResubmitRequest request);

    /// <summary>Cancela o pedido de compra.</summary>
    Task<PurchaseOrderResponse> CancelAsync(Guid id, ApprovalActionRequest request);
}
