using MedMatch.Domain.Common;

namespace MedMatch.Domain.Messaging;

public class Message : Entity
{
    public Guid MatchId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTimeOffset SentAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Message() { }

    public static Message Create(Guid matchId, Guid senderId, string content, DateTimeOffset now)
    {
        if (matchId == Guid.Empty) throw new DomainException("MatchId boş olamaz.");
        if (senderId == Guid.Empty) throw new DomainException("SenderId boş olamaz.");
        if (string.IsNullOrWhiteSpace(content)) throw new DomainException("Mesaj boş olamaz.");

        return new Message
        {
            MatchId = matchId,
            SenderId = senderId,
            Content = content.Trim(),
            SentAt = now
        };
    }

    public void MarkRead(DateTimeOffset now) => ReadAt ??= now;
}
