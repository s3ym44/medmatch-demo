using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Messaging;
using MediatR;

namespace MedMatch.Application.Features.Chat;

public sealed record SendMessageCommand(Guid UserId, Guid MatchId, SendMessageRequest Request) : ICommand<MessageDto>;

internal sealed class SendMessageHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;
    private readonly IClock _clock;

    public SendMessageHandler(IMatchRepository matches, IMessageRepository messages, IClock clock)
    {
        _matches = matches; _messages = messages; _clock = clock;
    }

    public async Task<MessageDto> Handle(SendMessageCommand cmd, CancellationToken ct)
    {
        var match = await _matches.RequireMembershipAsync(cmd.UserId, cmd.MatchId, ct);
        var message = Message.Create(match.Id, cmd.UserId, cmd.Request.Content, _clock.Now);
        await _messages.AddAsync(message, ct);
        return message.ToDto();
    }
}
