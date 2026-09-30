using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Profiles;
using MediatR;

namespace MedMatch.Application.Features.Discovery;

/// <summary>
/// Keşif adaylarını üretir. Dışlama kuralları tek yerde toplanır; "meslektaş/hasta görme"
/// gibi ileride eklenecek gizlilik filtreleri için <see cref="GetCandidatesHandler.PrivacyFilter"/> kancası bırakılmıştır.
/// </summary>
public sealed record GetCandidatesQuery(Guid UserId, int Take) : IQuery<IReadOnlyList<CandidateDto>>;

internal sealed class GetCandidatesHandler : IRequestHandler<GetCandidatesQuery, IReadOnlyList<CandidateDto>>
{
    private readonly IProfileRepository _profiles;
    private readonly ISwipeRepository _swipes;
    private readonly IClock _clock;

    public GetCandidatesHandler(IProfileRepository profiles, ISwipeRepository swipes, IClock clock)
    {
        _profiles = profiles; _swipes = swipes; _clock = clock;
    }

    public async Task<IReadOnlyList<CandidateDto>> Handle(GetCandidatesQuery q, CancellationToken ct)
    {
        var me = await _profiles.RequireByUserIdAsync(q.UserId, ct);
        if (me.VerificationStatus != VerificationStatus.Verified)
            throw AppException.Forbidden("Keşfe erişmek için profiliniz doğrulanmış olmalı.");

        var today = _clock.Today;
        var alreadySwiped = (await _swipes.GetSwipedTargetIdsAsync(q.UserId, ct)).ToHashSet();
        var all = await _profiles.GetAllAsync(ct);

        return all
            .Where(p => p.UserId != q.UserId)                                 // kendini dışla
            .Where(p => p.VerificationStatus == VerificationStatus.Verified)  // yalnızca doğrulanmışlar
            .Where(p => !alreadySwiped.Contains(p.UserId))                    // daha önce oy verilmemiş
            .Where(p => me.InterestedIn == Gender.Other || p.Gender == me.InterestedIn) // yönelim
            .Where(p => p.InterestedIn == Gender.Other || me.Gender == p.InterestedIn)  // karşılıklı yönelim
            .Where(p => { var a = p.AgeOn(today); return a >= me.AgeRange.Min && a <= me.AgeRange.Max; }) // yaş aralığı
            .Where(p => PrivacyFilter(me, p))                                 // gizlilik kancası (demo: her zaman true)
            .OrderBy(_ => Guid.NewGuid())
            .Take(q.Take <= 0 ? 20 : q.Take)
            .Select(p => new CandidateDto(
                p.Id, p.UserId, p.DisplayName, p.Profession, p.Gender, p.AgeOn(today), p.City, p.Bio,
                p.Photos.OrderBy(x => x.Order).Select(x => x.ToDto()).ToList()))
            .ToList();
    }

    /// <summary>İleride: aynı kurumdaki meslektaşı / hasta ilişkisini dışlama. Demo'da pasif.</summary>
    internal static bool PrivacyFilter(DoctorProfile me, DoctorProfile candidate) => true;
}
