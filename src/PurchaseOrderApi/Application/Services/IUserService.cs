using PurchaseOrderApi.Application.DTOs.Responses;

namespace PurchaseOrderApi.Application.Services;

/// <summary>
/// Contrato do serviço de aplicação para usuários.
/// </summary>
public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<UserResponse> GetByIdAsync(Guid id);
}
