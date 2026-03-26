using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Domain.Interfaces;

/// <summary>
/// Contrato do repositório de pedidos de compra.
/// Abstrai o acesso a dados, permitindo testabilidade e desacoplamento da infraestrutura.
/// </summary>
public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<PurchaseOrder>> GetAllAsync();
    Task AddAsync(PurchaseOrder purchaseOrder);
    Task UpdateAsync(PurchaseOrder purchaseOrder);
}
