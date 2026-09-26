using MedMatch.Application.Abstractions;
using MedMatch.Domain.Messaging;

namespace MedMatch.Infrastructure.Persistence.Repositories;

public sealed class MessageRepository : IMessageRepository
{
    private readonly InMemoryStore _store;
    public MessageRepository(InMemoryStore store) => _store = store;

    public Task<IReadOnlyList<Message>> GetForMatchAsync(Guid matchId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Message>>(_store.Messages.Values
            .Where(m => m.MatchId == matchId).OrderBy(m => m.SentAt).ToList());

    public Task AddAsync(Message message, CancellationToken ct = default)
    {
        _store.Messages[message.Id] = message;
        return Task.CompletedTask;
    }
}
