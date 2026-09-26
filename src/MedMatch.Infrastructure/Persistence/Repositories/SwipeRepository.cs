using MedMatch.Application.Abstractions;
using MedMatch.Domain.Matching;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class SwipeRepository : ISwipeRepository
{
    private readonly InMemoryStore _store;
    public SwipeRepository(InMemoryStore store) => _store = store;

    public Task<bool> ExistsAsync(Guid swiperId, Guid targetId, CancellationToken ct = default)
        => Task.FromResult(_store.Swipes.Values.Any(s => s.SwiperId == swiperId && s.TargetId == targetId));

    public Task<Swipe?> GetAsync(Guid swiperId, Guid targetId, CancellationToken ct = default)
        => Task.FromResult(_store.Swipes.Values.FirstOrDefault(s => s.SwiperId == swiperId && s.TargetId == targetId));

    public Task<IReadOnlyList<Guid>> GetSwipedTargetIdsAsync(Guid swiperId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Guid>>(_store.Swipes.Values
            .Where(s => s.SwiperId == swiperId).Select(s => s.TargetId).ToList());

    public Task AddAsync(Swipe swipe, CancellationToken ct = default)
    {
        _store.Swipes[swipe.Id] = swipe;
        return Task.CompletedTask;
    }
}
