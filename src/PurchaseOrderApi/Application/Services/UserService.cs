using PurchaseOrderApi.Application.DTOs.Responses;
using PurchaseOrderApi.Domain.Interfaces;

namespace PurchaseOrderApi.Application.Services;

/// <summary>
/// Serviço de aplicação para usuários.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(UserResponse.FromEntity);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Usuário com ID '{id}' não encontrado.");
        return UserResponse.FromEntity(user);
    }
}
