using PurchaseOrderApi.Domain.Enums;

namespace PurchaseOrderApi.Domain.Entities;

/// <summary>
/// Representa um usuário do sistema.
/// Contém apenas dados e validação simples — regras ficam no Service.
/// Relacionamentos: pode criar vários pedidos, realizar aprovações e gerar históricos.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserProfile Profile { get; set; }

    /// <summary>Pedidos criados por este usuário.</summary>
    public List<PurchaseOrder> CreatedOrders { get; set; } = new();

    /// <summary>Aprovações realizadas por este usuário.</summary>
    public List<Approval> Approvals { get; set; } = new();

    /// <summary>Registros de histórico realizados por este usuário.</summary>
    public List<OrderHistory> OrderHistories { get; set; } = new();

    private User() { }

    public User(string name, UserProfile profile)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do usuário é obrigatório.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Profile = profile;
    }
}
