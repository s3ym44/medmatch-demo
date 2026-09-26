using MedMatch.Domain.Messaging;

namespace MedMatch.Application.Abstractions;

public interface IMessageRepository
{
    Task<IReadOnlyList<Message>> GetForMatchAsync(Guid matchId, CancellationToken ct = default);
    Task AddAsync(Message message, CancellationToken ct = default);
}
