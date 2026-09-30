using MedMatch.Application.Abstractions;

namespace MedMatch.Infrastructure.Persistence;

/// <summary>InMemory mod için: transaction yok, iş doğrudan çalışır (hata durumunda geri alma da yok).</summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default) => work();
}
