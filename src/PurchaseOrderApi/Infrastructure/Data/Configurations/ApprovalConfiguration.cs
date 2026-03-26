using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração de mapeamento da entidade Approval.
/// Cada etapa de aprovação é um registro separado com seu próprio status.
/// </summary>
public class ApprovalConfiguration : IEntityTypeConfiguration<Approval>
{
    public void Configure(EntityTypeBuilder<Approval> builder)
    {
        builder.ToTable("Approvals");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Level)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.Comments)
            .HasMaxLength(1000);

        builder.HasIndex(a => a.PurchaseOrderId);
        builder.HasIndex(a => a.ApproverUserId);

        // Relacionamento: Approval pertence a um User (aprovador) — opcional
        builder.HasOne(a => a.ApproverUser)
            .WithMany(u => u.Approvals)
            .HasForeignKey(a => a.ApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
