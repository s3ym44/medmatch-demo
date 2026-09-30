using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MediatR;

namespace MedMatch.Application.Features.Chat;

public sealed record GetMessagesQuery(Guid UserId, Guid MatchId) : IQuery<IReadOnlyList<MessageDto>>;

internal sealed class GetMessagesHandler : IRequestHandler<GetMessagesQuery, IReadOnlyList<MessageDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;

    public GetMessagesHandler(IMatchRepository matches, IMessageRepository messages)
    {
        _matches = matches; _messages = messages;
    }

    public async Task<IReadOnlyList<MessageDto>> Handle(GetMessagesQuery q, CancellationToken ct)
    {
        var match = await _matches.RequireMembershipAsync(q.UserId, q.MatchId, ct);
        var msgs = await _messages.GetForMatchAsync(match.Id, ct);
        return msgs.OrderBy(x => x.SentAt).Select(x => x.ToDto()).ToList();
    }
}
