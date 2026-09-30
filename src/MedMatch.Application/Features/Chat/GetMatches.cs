using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MediatR;

namespace MedMatch.Application.Features.Chat;

public sealed record GetMatchesQuery(Guid UserId) : IQuery<IReadOnlyList<MatchDto>>;

internal sealed class GetMatchesHandler : IRequestHandler<GetMatchesQuery, IReadOnlyList<MatchDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;
    private readonly IProfileRepository _profiles;

    public GetMatchesHandler(IMatchRepository matches, IMessageRepository messages, IProfileRepository profiles)
    {
        _matches = matches; _messages = messages; _profiles = profiles;
    }

    public async Task<IReadOnlyList<MatchDto>> Handle(GetMatchesQuery q, CancellationToken ct)
    {
        var matches = await _matches.GetForUserAsync(q.UserId, ct);
        var result = new List<MatchDto>();
        foreach (var m in matches.Where(x => x.IsActive))
        {
            var otherId = m.Other(q.UserId);
            var other = await _profiles.GetByUserIdAsync(otherId, ct);
            var msgs = await _messages.GetForMatchAsync(m.Id, ct);
            var last = msgs.OrderByDescending(x => x.SentAt).FirstOrDefault();
            result.Add(new MatchDto(
                m.Id, otherId,
                other?.DisplayName ?? "Bilinmeyen",
                other?.PrimaryPhotoUrl(),
                m.CreatedAt,
                last?.Content,
                last?.SentAt));
        }
        return result.OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt).ToList();
    }
}
