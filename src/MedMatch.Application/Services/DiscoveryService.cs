using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Profiles;

namespace MedMatch.Application.Services;

/// <summary>
/// Keşif adaylarını üretir. Dışlama kuralları tek yerde toplanır; "meslektaş/hasta görme"
/// gibi ileride eklenecek gizlilik filtreleri için <see cref="PrivacyFilter"/> kancası bırakılmıştır.
/// </summary>
public sealed class DiscoveryService
{
    private readonly IProfileRepository _profiles;
    private readonly ISwipeRepository _swipes;
    private readonly IClock _clock;

    public DiscoveryService(IProfileRepository profiles, ISwipeRepository swipes, IClock clock)
    {
        _profiles = profiles; _swipes = swipes; _clock = clock;
    }

    public async Task<IReadOnlyList<CandidateDto>> GetCandidatesAsync(Guid userId, int take, CancellationToken ct = default)
    {
        var me = await _profiles.GetByUserIdAsync(userId, ct)
            ?? throw AppException.NotFound("Önce profil oluşturmalısınız.");
        if (me.VerificationStatus != VerificationStatus.Verified)
            throw AppException.Forbidden("Keşfe erişmek için profiliniz doğrulanmış olmalı.");

        var today = _clock.Today;
        var alreadySwiped = (await _swipes.GetSwipedTargetIdsAsync(userId, ct)).ToHashSet();
        var all = await _profiles.GetAllAsync(ct);

        var candidates = all
            .Where(p => p.UserId != userId)                                   // kendini dışla
            .Where(p => p.VerificationStatus == VerificationStatus.Verified)  // yalnızca doğrulanmışlar
            .Where(p => !alreadySwiped.Contains(p.UserId))                    // daha önce oy verilmemiş
            .Where(p => me.InterestedIn == Gender.Other || p.Gender == me.InterestedIn) // yönelim
            .Where(p => p.InterestedIn == Gender.Other || me.Gender == p.InterestedIn)  // karşılıklı yönelim
            .Where(p => { var a = p.AgeOn(today); return a >= me.AgeRange.Min && a <= me.AgeRange.Max; }) // yaş aralığı
            .Where(p => PrivacyFilter(me, p))                                 // gizlilik kancası (demo: her zaman true)
            .OrderBy(_ => Guid.NewGuid())
            .Take(take <= 0 ? 20 : take)
            .Select(p => new CandidateDto(
                p.Id, p.UserId, p.DisplayName, p.Profession, p.Gender, p.AgeOn(today), p.City, p.Bio,
                p.Photos.OrderBy(x => x.Order).Select(x => x.ToDto()).ToList()))
            .ToList();

        return candidates;
    }

    /// <summary>İleride: aynı kurumdaki meslektaşı / hasta ilişkisini dışlama. Demo'da pasif.</summary>
    private static bool PrivacyFilter(DoctorProfile me, DoctorProfile candidate) => true;
}
