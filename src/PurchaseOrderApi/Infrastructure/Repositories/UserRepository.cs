using Microsoft.EntityFrameworkCore;
using PurchaseOrderApi.Domain.Entities;
using PurchaseOrderApi.Domain.Interfaces;
using PurchaseOrderApi.Infrastructure.Data;

namespace PurchaseOrderApi.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de usuários usando Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .OrderBy(u => u.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
