using MedMatch.Application.Abstractions;
using MedMatch.Domain.Verification;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class VerificationRepository : IVerificationRepository
{
    private readonly InMemoryStore _store;
    public VerificationRepository(InMemoryStore store) => _store = store;

    public Task<VerificationRequest?> GetLatestForUserAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(_store.Verifications.Values
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefault());

    public Task AddAsync(VerificationRequest request, CancellationToken ct = default)
    {
        _store.Verifications[request.Id] = request;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(VerificationRequest request, CancellationToken ct = default)
    {
        _store.Verifications[request.Id] = request;
        return Task.CompletedTask;
    }
}
