using MedMatch.Application.Abstractions;
using MedMatch.Domain.Verification;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class EfVerificationRepository : IVerificationRepository
{
    private readonly AppDbContext _db;
    public EfVerificationRepository(AppDbContext db) => _db = db;

    public Task<VerificationRequest?> GetLatestForUserAsync(Guid userId, CancellationToken ct = default)
        => _db.Verifications
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(VerificationRequest request, CancellationToken ct = default)
    {
        _db.Verifications.Add(request);
        await _db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(VerificationRequest request, CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
