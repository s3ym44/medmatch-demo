using MedMatch.Application.Abstractions;
using MedMatch.Domain.Matching;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class MatchRepository : IMatchRepository
{
    private readonly InMemoryStore _store;
    public MatchRepository(InMemoryStore store) => _store = store;

    public Task<Match?> GetByIdAsync(Guid matchId, CancellationToken ct = default)
        => Task.FromResult(_store.Matches.TryGetValue(matchId, out var m) ? m : null);

    public Task<Match?> GetBetweenAsync(Guid userX, Guid userY, CancellationToken ct = default)
    {
        var (a, b) = userX.CompareTo(userY) < 0 ? (userX, userY) : (userY, userX);
        return Task.FromResult(_store.Matches.Values.FirstOrDefault(m => m.UserAId == a && m.UserBId == b));
    }

    public Task<IReadOnlyList<Match>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Match>>(_store.Matches.Values.Where(m => m.Involves(userId)).ToList());

    public Task AddAsync(Match match, CancellationToken ct = default)
    {
        _store.Matches[match.Id] = match;
        return Task.CompletedTask;
    }
}
