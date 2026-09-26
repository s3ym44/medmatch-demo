using MedMatch.Application.Abstractions;
using MedMatch.Domain.Profiles;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class ProfileRepository : IProfileRepository
{
    private readonly InMemoryStore _store;
    public ProfileRepository(InMemoryStore store) => _store = store;

    public Task<DoctorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(_store.Profiles.Values.FirstOrDefault(p => p.UserId == userId));

    public Task<DoctorProfile?> GetByIdAsync(Guid profileId, CancellationToken ct = default)
        => Task.FromResult(_store.Profiles.TryGetValue(profileId, out var p) ? p : null);

    public Task<IReadOnlyList<DoctorProfile>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<DoctorProfile>>(_store.Profiles.Values.ToList());

    public Task AddAsync(DoctorProfile profile, CancellationToken ct = default)
    {
        _store.Profiles[profile.Id] = profile;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DoctorProfile profile, CancellationToken ct = default)
    {
        _store.Profiles[profile.Id] = profile;
        return Task.CompletedTask;
    }
}
