using PurchaseOrderApi.Application.Services;
using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Application.DTOs.Responses;

/// <summary>
/// DTO de saída com os dados completos de um pedido de compra.
/// </summary>
public class PurchaseOrderResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RequiredApprovalLevel { get; set; } = string.Empty;
    public UserSummaryResponse Creator { get; set; } = null!;
    public List<OrderItemResponse> Items { get; set; } = new();
    public List<ApprovalResponse> Approvals { get; set; } = new();
    public List<OrderHistoryResponse> History { get; set; } = new();

    /// <summary>
    /// Converte a entidade de domínio para o DTO de resposta.
    /// </summary>
    public static PurchaseOrderResponse FromEntity(PurchaseOrder entity)
    {
        return new PurchaseOrderResponse
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            TotalValue = entity.TotalValue,
            Status = entity.Status.ToString(),
            RequiredApprovalLevel = PurchaseOrderService.GetRequiredApprovalLevel(entity.TotalValue).ToString(),
            Creator = UserSummaryResponse.FromEntity(entity.CreatorUser),
            Items = entity.Items.Select(OrderItemResponse.FromEntity).ToList(),
            Approvals = entity.Approvals
                .OrderBy(a => a.Level)
                .Select(ApprovalResponse.FromEntity)
                .ToList(),
            History = entity.History
                .OrderBy(h => h.CreatedAt)
                .Select(OrderHistoryResponse.FromEntity)
                .ToList()
        };
    }
}

/// <summary>
/// DTO de saída para um item do pedido.
/// </summary>
public class OrderItemResponse
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public static OrderItemResponse FromEntity(OrderItem entity)
    {
        return new OrderItemResponse
        {
            Id = entity.Id,
            ProductName = entity.ProductName,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            TotalPrice = entity.TotalPrice
        };
    }
}

/// <summary>
/// DTO de saída para uma etapa de aprovação.
/// </summary>
public class ApprovalResponse
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public UserSummaryResponse? Approver { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Comments { get; set; }

    public static ApprovalResponse FromEntity(Approval entity)
    {
        return new ApprovalResponse
        {
            Id = entity.Id,
            Level = entity.Level.ToString(),
            Status = entity.Status.ToString(),
            Approver = entity.ApproverUser != null ? UserSummaryResponse.FromEntity(entity.ApproverUser) : null,
            ApprovalDate = entity.ApprovalDate,
            Comments = entity.Comments
        };
    }
}

/// <summary>
/// DTO de saída para um registro de histórico do pedido.
/// </summary>
public class OrderHistoryResponse
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public UserSummaryResponse User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;

    public static OrderHistoryResponse FromEntity(OrderHistory entity)
    {
        return new OrderHistoryResponse
        {
            Id = entity.Id,
            Action = entity.Action.ToString(),
            User = UserSummaryResponse.FromEntity(entity.User),
            CreatedAt = entity.CreatedAt,
            Description = entity.Description
        };
    }
}

/// <summary>
/// DTO resumido de um usuário (para exibição em respostas).
/// </summary>
public class UserSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;

    public static UserSummaryResponse? FromEntity(User? entity)
    {
        if (entity == null) return null;

        return new UserSummaryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Profile = entity.Profile.ToString()
        };
    }
}
