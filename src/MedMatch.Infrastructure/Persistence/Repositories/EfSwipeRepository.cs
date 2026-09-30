using MedMatch.Application.Abstractions;
using MedMatch.Domain.Matching;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class EfSwipeRepository : ISwipeRepository
{
    private readonly AppDbContext _db;
    public EfSwipeRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid swiperId, Guid targetId, CancellationToken ct = default)
        => _db.Swipes.AnyAsync(s => s.SwiperId == swiperId && s.TargetId == targetId, ct);

    public Task<Swipe?> GetAsync(Guid swiperId, Guid targetId, CancellationToken ct = default)
        => _db.Swipes.FirstOrDefaultAsync(s => s.SwiperId == swiperId && s.TargetId == targetId, ct);

    public async Task<IReadOnlyList<Guid>> GetSwipedTargetIdsAsync(Guid swiperId, CancellationToken ct = default)
        => await _db.Swipes.Where(s => s.SwiperId == swiperId).Select(s => s.TargetId).ToListAsync(ct);

    public async Task AddAsync(Swipe swipe, CancellationToken ct = default)
    {
        _db.Swipes.Add(swipe);
        await _db.SaveChangesAsync(ct);
    }
}
