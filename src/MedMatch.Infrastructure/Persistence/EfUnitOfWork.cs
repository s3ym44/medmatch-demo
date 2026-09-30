using MedMatch.Application.Abstractions;

namespace MedMatch.Infrastructure.Persistence;

/// <summary>
/// Command'ı tek veritabanı transaction'ında çalıştırır. Repository'ler her yazmada SaveChanges çağırsa da
/// commit en sonda olur; hata olursa transaction dispose edilirken geri alınır.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public EfUnitOfWork(AppDbContext db) => _db = db;

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default)
    {
        if (_db.Database.CurrentTransaction is not null)
            return await work(); // iç içe command: dıştaki transaction'a katıl

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var result = await work();
        await tx.CommitAsync(ct);
        return result;
    }
}
