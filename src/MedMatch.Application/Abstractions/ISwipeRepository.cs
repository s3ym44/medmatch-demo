using MedMatch.Domain.Enums;
using MedMatch.Domain.Matching;

namespace MedMatch.Application.Abstractions;

public interface ISwipeRepository
{
    Task<bool> ExistsAsync(Guid swiperId, Guid targetId, CancellationToken ct = default);
    Task<Swipe?> GetAsync(Guid swiperId, Guid targetId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetSwipedTargetIdsAsync(Guid swiperId, CancellationToken ct = default);
    Task AddAsync(Swipe swipe, CancellationToken ct = default);
}
