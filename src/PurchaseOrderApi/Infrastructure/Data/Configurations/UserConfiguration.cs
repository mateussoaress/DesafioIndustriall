using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração de mapeamento da entidade User para o Entity Framework Core.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Profile)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(u => u.Name);
        builder.HasIndex(u => u.Profile);

        builder.Navigation(u => u.CreatedOrders).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.Approvals).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.OrderHistories).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
