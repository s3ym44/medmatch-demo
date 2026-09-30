namespace MedMatch.Application.Abstractions;

/// <summary>
/// Bir command'ın tüm yazmalarını tek transaction'da toplar: ya hepsi kalıcı olur ya hiçbiri.
/// EF: veritabanı transaction'ı. InMemory: doğrudan çalıştırır (geri alma yok).
/// </summary>
public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default);
}
