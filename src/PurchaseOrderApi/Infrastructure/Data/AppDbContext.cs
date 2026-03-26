using Microsoft.EntityFrameworkCore;
using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Infrastructure.Data;

/// <summary>
/// Contexto do Entity Framework Core para a aplicação.
/// Gerencia o mapeamento das entidades e a conexão com o SQL Server.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
