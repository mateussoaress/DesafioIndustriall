using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração de mapeamento da entidade PurchaseOrder para o Entity Framework Core.
/// Define chave primária, campos obrigatórios, índices e relacionamentos.
/// </summary>
public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.TotalValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatorUserId).IsRequired();

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatorUserId);

        // Relacionamento: PurchaseOrder pertence a um User (criador)
        builder.HasOne(p => p.CreatorUser)
            .WithMany(u => u.CreatedOrders)
            .HasForeignKey(p => p.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento: PurchaseOrder contém 1+ OrderItems
        builder.HasMany(p => p.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento: PurchaseOrder possui 0+ Approvals
        builder.HasMany(p => p.Approvals)
            .WithOne(a => a.PurchaseOrder)
            .HasForeignKey(a => a.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento: PurchaseOrder possui 0+ OrderHistories
        builder.HasMany(p => p.History)
            .WithOne(h => h.PurchaseOrder)
            .HasForeignKey(h => h.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
