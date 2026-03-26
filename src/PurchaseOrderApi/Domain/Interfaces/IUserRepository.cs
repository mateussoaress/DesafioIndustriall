using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Domain.Interfaces;

/// <summary>
/// Contrato do repositório de usuários.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
}
