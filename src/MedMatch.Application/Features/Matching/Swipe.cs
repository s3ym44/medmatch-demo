using FluentValidation;
using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Matching;
using MediatR;

namespace MedMatch.Application.Features.Matching;

/// <summary>Swipe kaydeder; karşılıklı Like durumunda eşleşme üretir. Swipe ve eşleşme tek transaction'da.</summary>
public sealed record SwipeCommand(Guid UserId, SwipeRequest Request) : ICommand<SwipeResultDto>;

internal sealed class SwipeValidator : AbstractValidator<SwipeCommand>
{
    public SwipeValidator()
    {
        RuleFor(x => x.Request.TargetUserId)
            .NotEqual(x => x.UserId).WithMessage("Kendinize oy veremezsiniz.");
    }
}

internal sealed class SwipeHandler : IRequestHandler<SwipeCommand, SwipeResultDto>
{
    private readonly ISwipeRepository _swipes;
    private readonly IMatchRepository _matches;
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;

    public SwipeHandler(ISwipeRepository swipes, IMatchRepository matches, IProfileRepository profiles, IClock clock)
    {
        _swipes = swipes; _matches = matches; _profiles = profiles; _clock = clock;
    }

    public async Task<SwipeResultDto> Handle(SwipeCommand cmd, CancellationToken ct)
    {
        var (userId, req) = (cmd.UserId, cmd.Request);

        var me = await _profiles.RequireByUserIdAsync(userId, ct);
        if (me.VerificationStatus != VerificationStatus.Verified)
            throw AppException.Forbidden("Oy vermek için profiliniz doğrulanmış olmalı.");

        _ = await _profiles.GetByUserIdAsync(req.TargetUserId, ct)
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
