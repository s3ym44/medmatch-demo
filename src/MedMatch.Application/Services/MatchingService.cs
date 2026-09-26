using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Matching;

namespace MedMatch.Application.Services;

/// <summary>Swipe kaydeder; karşılıklı Like durumunda eşleşme üretir.</summary>
public sealed class MatchingService
{
    private readonly ISwipeRepository _swipes;
    private readonly IMatchRepository _matches;
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;

    public MatchingService(ISwipeRepository swipes, IMatchRepository matches, IProfileRepository profiles, IClock clock)
    {
        _swipes = swipes; _matches = matches; _profiles = profiles; _clock = clock;
    }

    public async Task<SwipeResultDto> SwipeAsync(Guid userId, SwipeRequest req, CancellationToken ct = default)
    {
        if (req.TargetUserId == userId)
            throw AppException.Validation("Kendinize oy veremezsiniz.");

        var me = await _profiles.GetByUserIdAsync(userId, ct)
            ?? throw AppException.NotFound("Önce profil oluşturmalısınız.");
        if (me.VerificationStatus != VerificationStatus.Verified)
            throw AppException.Forbidden("Oy vermek için profiliniz doğrulanmış olmalı.");

        var target = await _profiles.GetByUserIdAsync(req.TargetUserId, ct)
            ?? throw AppException.NotFound("Hedef kullanıcı bulunamadı.");

        if (await _swipes.ExistsAsync(userId, req.TargetUserId, ct))
            throw AppException.Conflict("Bu kişiye zaten oy verdiniz.");

        var swipe = Swipe.Create(userId, req.TargetUserId, req.Decision, _clock.Now);
        await _swipes.AddAsync(swipe, ct);

        if (req.Decision != SwipeDecision.Like)
            return new SwipeResultDto(false, null);

        // karşı taraf da Like vermiş mi?
        var reciprocal = await _swipes.GetAsync(req.TargetUserId, userId, ct);
        if (reciprocal is null || reciprocal.Decision != SwipeDecision.Like)
            return new SwipeResultDto(false, null);

        // zaten eşleşme var mı? (idempotent güvence)
        var existing = await _matches.GetBetweenAsync(userId, req.TargetUserId, ct);
        if (existing is not null)
            return new SwipeResultDto(true, existing.Id);

        var match = Match.Create(userId, req.TargetUserId, _clock.Now);
        await _matches.AddAsync(match, ct);
        return new SwipeResultDto(true, match.Id);
    }
}
