using MedMatch.Application.Abstractions;
using MedMatch.Domain.Messaging;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class EfMessageRepository : IMessageRepository
{
    private readonly AppDbContext _db;
    public EfMessageRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Message>> GetForMatchAsync(Guid matchId, CancellationToken ct = default)
        => await _db.Messages.Where(m => m.MatchId == matchId).OrderBy(m => m.SentAt).ToListAsync(ct);

    public async Task AddAsync(Message message, CancellationToken ct = default)
    {
        _db.Messages.Add(message);
        await _db.SaveChangesAsync(ct);
    }
}
