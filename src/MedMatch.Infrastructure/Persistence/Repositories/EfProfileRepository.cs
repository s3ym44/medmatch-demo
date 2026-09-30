using MedMatch.Application.Abstractions;
using MedMatch.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class EfProfileRepository : IProfileRepository
{
    private readonly AppDbContext _db;
    public EfProfileRepository(AppDbContext db) => _db = db;

    public Task<DoctorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _db.Profiles.Include(p => p.Photos).Include(p => p.Prompts).FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<DoctorProfile?> GetByIdAsync(Guid profileId, CancellationToken ct = default)
        => _db.Profiles.Include(p => p.Photos).Include(p => p.Prompts).FirstOrDefaultAsync(p => p.Id == profileId, ct);

    // DiscoveryService filtreyi bellekte yapıyor; ölçek büyüyünce Where'ler buraya taşınmalı
    public async Task<IReadOnlyList<DoctorProfile>> GetAllAsync(CancellationToken ct = default)
        => await _db.Profiles.Include(p => p.Photos).Include(p => p.Prompts).ToListAsync(ct);

    public async Task AddAsync(DoctorProfile profile, CancellationToken ct = default)
    {
        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync(ct);
    }

    // Entity aynı scope'ta yüklendiği için izleniyor; yeni fotolar DetectChanges ile Added olur
    public Task UpdateAsync(DoctorProfile profile, CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
