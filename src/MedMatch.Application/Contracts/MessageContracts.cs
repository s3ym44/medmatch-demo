namespace MedMatch.Application.Contracts;

public sealed record SendMessageRequest(string Content);
public sealed record MessageDto(Guid Id, Guid MatchId, Guid SenderId, string Content, DateTimeOffset SentAt, DateTimeOffset? ReadAt);
