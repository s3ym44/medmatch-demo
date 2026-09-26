using MedMatch.Domain.Common;
using MedMatch.Domain.Enums;

namespace MedMatch.Domain.Matching;

/// <summary>Bir kullanıcının bir hedefe verdiği oy. (SwiperId, TargetId) tekildir.</summary>
public class Swipe : Entity
{
    public Guid SwiperId { get; private set; }
    public Guid TargetId { get; private set; }
    public SwipeDecision Decision { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Swipe() { }

    public static Swipe Create(Guid swiperId, Guid targetId, SwipeDecision decision, DateTimeOffset now)
    {
        if (swiperId == Guid.Empty || targetId == Guid.Empty)
            throw new DomainException("Swipe için kullanıcı id'leri boş olamaz.");
        if (swiperId == targetId)
            throw new DomainException("Kullanıcı kendine oy veremez.");

        return new Swipe
        {
            SwiperId = swiperId,
            TargetId = targetId,
            Decision = decision,
            CreatedAt = now
        };
    }
}
