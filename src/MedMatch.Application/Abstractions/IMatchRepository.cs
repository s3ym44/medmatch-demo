using MedMatch.Domain.Matching;

namespace MedMatch.Application.Abstractions;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid matchId, CancellationToken ct = default);
    Task<Match?> GetBetweenAsync(Guid userX, Guid userY, CancellationToken ct = default);
    Task<IReadOnlyList<Match>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Match match, CancellationToken ct = default);
}
