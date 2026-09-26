using MedMatch.Application.Abstractions;
using MedMatch.Domain.Users;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly InMemoryStore _store;
    public UserRepository(InMemoryStore store) => _store = store;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.Users.TryGetValue(id, out var u) ? u : null);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var norm = email.Trim().ToLowerInvariant();
        return Task.FromResult(_store.Users.Values.FirstOrDefault(u => u.Email == norm));
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var norm = email.Trim().ToLowerInvariant();
        return Task.FromResult(_store.Users.Values.Any(u => u.Email == norm));
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _store.Users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _store.Users[user.Id] = user;
        return Task.CompletedTask;
    }
}
