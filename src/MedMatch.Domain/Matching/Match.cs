using MedMatch.Domain.Common;

namespace MedMatch.Domain.Matching;

/// <summary>
/// İki kullanıcı arasındaki eşleşme. Kanonik sıra korunur (UserAId &lt; UserBId), böylece
/// "A-B" ve "B-A" iki ayrı eşleşme oluşturamaz.
/// </summary>
public class Match : Entity
{
    public Guid UserAId { get; private set; }
    public Guid UserBId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Match() { }

    public static Match Create(Guid x, Guid y, DateTimeOffset now)
    {
        if (x == Guid.Empty || y == Guid.Empty)
            throw new DomainException("Eşleşme için kullanıcı id'leri boş olamaz.");
        if (x == y)
            throw new DomainException("Bir kullanıcı kendisiyle eşleşemez.");

        var (a, b) = x.CompareTo(y) < 0 ? (x, y) : (y, x);
        return new Match
        {
            UserAId = a,
            UserBId = b,
            CreatedAt = now,
            IsActive = true
        };
    }

    public bool Involves(Guid userId) => UserAId == userId || UserBId == userId;
    public Guid Other(Guid userId) => UserAId == userId ? UserBId : UserAId;
    public void Deactivate() => IsActive = false;
}
