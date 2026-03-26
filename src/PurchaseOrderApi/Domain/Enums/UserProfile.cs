namespace PurchaseOrderApi.Domain.Enums;

/// <summary>
/// Representa o perfil/papel do usuário no sistema.
/// Define a autoridade do usuário no fluxo de aprovação de pedidos.
/// </summary>
public enum UserProfile
{
    /// <summary>Colaborador — pode criar e editar pedidos.</summary>
    Collaborator = 0,

    /// <summary>Suprimentos — primeiro nível de aprovação.</summary>
    Supplies = 1,

    /// <summary>Gestor — segundo nível de aprovação.</summary>
    Manager = 2,

    /// <summary>Diretor — terceiro e último nível de aprovação.</summary>
    Director = 3
}
