using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração de mapeamento da entidade OrderItem.
/// Define tipos de dados para valores monetários e quantidades.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Propriedade calculada — não mapeada para coluna
        builder.Ignore(i => i.TotalPrice);

        builder.HasIndex(i => i.PurchaseOrderId);
    }
}
