using Microsoft.EntityFrameworkCore;
using PurchaseOrderApi.Domain.Entities;
using PurchaseOrderApi.Domain.Interfaces;
using PurchaseOrderApi.Infrastructure.Data;

namespace PurchaseOrderApi.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de pedidos de compra usando Entity Framework Core.
/// Carrega itens, aprovações e histórico junto ao pedido para garantir consistência.
/// </summary>
public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _context;

    public PurchaseOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id)
    {
        return await _context.PurchaseOrders
            .Include(p => p.CreatorUser)
            .Include(p => p.Items)
            .Include(p => p.Approvals)
                .ThenInclude(a => a.ApproverUser)
            .Include(p => p.History)
                .ThenInclude(h => h.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
    {
        return await _context.PurchaseOrders
            .Include(p => p.CreatorUser)
            .Include(p => p.Items)
            .Include(p => p.Approvals)
                .ThenInclude(a => a.ApproverUser)
            .Include(p => p.History)
                .ThenInclude(h => h.User)
            .AsSplitQuery()
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder)
    {
        await _context.PurchaseOrders.AddAsync(purchaseOrder);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    public async Task UpdateAsync(PurchaseOrder purchaseOrder)
    {
        _context.PurchaseOrders.Update(purchaseOrder);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }
}
