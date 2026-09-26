using MedMatch.Domain.Enums;

namespace MedMatch.Application.Contracts;

public sealed record SwipeRequest(Guid TargetUserId, SwipeDecision Decision);
public sealed record SwipeResultDto(bool Matched, Guid? MatchId);

public sealed record MatchDto(
    Guid MatchId,
    Guid OtherUserId,
    string OtherDisplayName,
    string? OtherPhotoUrl,
    DateTimeOffset CreatedAt,
    string? LastMessage,
    DateTimeOffset? LastMessageAt);
