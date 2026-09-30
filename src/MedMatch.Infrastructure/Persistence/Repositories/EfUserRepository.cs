using MedMatch.Application.Abstractions;
using MedMatch.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public EfUserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var norm = email.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(u => u.Email == norm, ct);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var norm = email.Trim().ToLowerInvariant();
        return _db.Users.AnyAsync(u => u.Email == norm, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
    }

    // Entity aynı scope'ta yüklendiği için context tarafından izleniyor -> SaveChanges yeter
    public Task UpdateAsync(User user, CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
