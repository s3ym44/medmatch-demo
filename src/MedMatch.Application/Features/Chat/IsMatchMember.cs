using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MediatR;

namespace MedMatch.Application.Features.Chat;

/// <summary>SignalR hub'ı sohbet grubuna katılmadan önce üyeliği bununla kontrol eder.</summary>
public sealed record IsMatchMemberQuery(Guid UserId, Guid MatchId) : IQuery<bool>;

internal sealed class IsMatchMemberHandler : IRequestHandler<IsMatchMemberQuery, bool>
{
    private readonly IMatchRepository _matches;
    public IsMatchMemberHandler(IMatchRepository matches) => _matches = matches;

    public async Task<bool> Handle(IsMatchMemberQuery q, CancellationToken ct)
    {
        var match = await _matches.GetByIdAsync(q.MatchId, ct);
        return match is not null && match.Involves(q.UserId);
    }
}
