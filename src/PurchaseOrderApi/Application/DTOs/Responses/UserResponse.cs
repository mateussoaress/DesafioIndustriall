using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Application.DTOs.Responses;

/// <summary>
/// DTO de saída com os dados de um usuário.
/// </summary>
public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;

    public static UserResponse FromEntity(User entity)
    {
        return new UserResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Profile = entity.Profile.ToString()
        };
    }
}
