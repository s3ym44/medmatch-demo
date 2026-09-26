using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Matching;
using MedMatch.Domain.Messaging;

namespace MedMatch.Application.Services;

public sealed class ChatService
{
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;

    public ChatService(IMatchRepository matches, IMessageRepository messages, IProfileRepository profiles, IClock clock)
    {
        _matches = matches; _messages = messages; _profiles = profiles; _clock = clock;
    }

    public async Task<IReadOnlyList<MatchDto>> GetMatchesAsync(Guid userId, CancellationToken ct = default)
    {
        var matches = await _matches.GetForUserAsync(userId, ct);
        var result = new List<MatchDto>();
        foreach (var m in matches.Where(x => x.IsActive))
        {
            var otherId = m.Other(userId);
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

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(Guid userId, Guid matchId, CancellationToken ct = default)
    {
        var match = await RequireMembership(userId, matchId, ct);
        var msgs = await _messages.GetForMatchAsync(match.Id, ct);
        return msgs.OrderBy(x => x.SentAt).Select(ToDto).ToList();
    }

    public async Task<MessageDto> SendAsync(Guid userId, Guid matchId, SendMessageRequest req, CancellationToken ct = default)
    {
        var match = await RequireMembership(userId, matchId, ct);
        var message = Message.Create(match.Id, userId, req.Content, _clock.Now);
        await _messages.AddAsync(message, ct);
        return ToDto(message);
    }

    public async Task<bool> IsMemberAsync(Guid userId, Guid matchId, CancellationToken ct = default)
    {
        var match = await _matches.GetByIdAsync(matchId, ct);
        return match is not null && match.Involves(userId);
    }

    private async Task<Match> RequireMembership(Guid userId, Guid matchId, CancellationToken ct)
    {
        var match = await _matches.GetByIdAsync(matchId, ct)
            ?? throw AppException.NotFound("Eşleşme bulunamadı.");
        if (!match.Involves(userId))
            throw AppException.Forbidden("Bu sohbete erişim yetkiniz yok.");
        return match;
    }

    private static MessageDto ToDto(Message m) =>
        new(m.Id, m.MatchId, m.SenderId, m.Content, m.SentAt, m.ReadAt);
}
