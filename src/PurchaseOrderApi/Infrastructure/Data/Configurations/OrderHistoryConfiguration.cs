using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração de mapeamento da entidade OrderHistory.
/// Garante que o registro de histórico é imutável e rastreável.
/// </summary>
public class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
{
    public void Configure(EntityTypeBuilder<OrderHistory> builder)
    {
        builder.ToTable("OrderHistories");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.UserId)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.Property(h => h.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasIndex(h => h.PurchaseOrderId);
        builder.HasIndex(h => h.UserId);
        builder.HasIndex(h => h.Action);

        // Relacionamento: OrderHistory pertence a um User
        builder.HasOne(h => h.User)
            .WithMany(u => u.OrderHistories)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
