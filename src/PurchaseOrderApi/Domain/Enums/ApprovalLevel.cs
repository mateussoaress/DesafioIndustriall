namespace PurchaseOrderApi.Domain.Enums;

/// <summary>
/// Representa os níveis de aprovação hierárquica no fluxo de pedido de compras.
/// A ordem dos valores reflete a sequência obrigatória de aprovação.
/// </summary>
public enum ApprovalLevel
{
    /// <summary>Área de Suprimentos — primeiro nível de aprovação.</summary>
    Supplies = 0,

    /// <summary>Gestor — segundo nível de aprovação.</summary>
    Manager = 1,

    /// <summary>Diretor — terceiro e último nível de aprovação.</summary>
    Director = 2
}
