using MedMatch.Domain.Common;
using MedMatch.Domain.Enums;

namespace MedMatch.Domain.Verification;

/// <summary>
/// Doğrulama talebi. Sağlayıcıya özgü hiçbir alan içermez; yalnızca opak bir belge referansı
/// ve durum tutar. Gerçek entegrasyon geldiğinde bu tip değişmez, yalnızca arkadaki servis değişir.
/// </summary>
public class VerificationRequest : Entity
{
    public Guid UserId { get; private set; }
    public VerificationMethod Method { get; private set; }
    public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;
    public string? DocumentRef { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }

    private VerificationRequest() { }

    public static VerificationRequest Create(Guid userId, VerificationMethod method, string? documentRef, DateTimeOffset now)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId boş olamaz.");
        return new VerificationRequest
        {
            UserId = userId,
            Method = method,
            Status = VerificationStatus.Pending,
            DocumentRef = documentRef,
            CreatedAt = now
        };
    }

    public void Approve(DateTimeOffset now)
    {
        Status = VerificationStatus.Verified;
        ReviewedAt = now;
    }

    public void Reject(DateTimeOffset now)
    {
        Status = VerificationStatus.Rejected;
        ReviewedAt = now;
    }
}
