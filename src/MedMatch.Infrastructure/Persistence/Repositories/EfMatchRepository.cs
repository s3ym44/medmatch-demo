using MedMatch.Application.Abstractions;
using MedMatch.Domain.Matching;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class EfMatchRepository : IMatchRepository
{
    private readonly AppDbContext _db;
    public EfMatchRepository(AppDbContext db) => _db = db;

    public Task<Match?> GetByIdAsync(Guid matchId, CancellationToken ct = default)
        => _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId, ct);

    public Task<Match?> GetBetweenAsync(Guid userX, Guid userY, CancellationToken ct = default)
    {
        var (a, b) = userX.CompareTo(userY) < 0 ? (userX, userY) : (userY, userX);
        return _db.Matches.FirstOrDefaultAsync(m => m.UserAId == a && m.UserBId == b, ct);
    }

    // Match.Involves SQL'e çevrilemez; koşul açık yazılır
    public async Task<IReadOnlyList<Match>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Matches.Where(m => m.UserAId == userId || m.UserBId == userId).ToListAsync(ct);

    public async Task AddAsync(Match match, CancellationToken ct = default)
    {
        _db.Matches.Add(match);
        await _db.SaveChangesAsync(ct);
    }
}
