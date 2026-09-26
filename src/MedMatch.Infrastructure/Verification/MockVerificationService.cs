using MedMatch.Application.Abstractions;
using MedMatch.Domain.Enums;

namespace MedMatch.Infrastructure.Verification;

/// <summary>
/// Sahte doğrulama sağlayıcısı. Gerçek e-Devlet barkodlu belge / kurumsal e-posta / sicil
/// sorgusu YERİNE geçer. Kısa bir gecikmeyle "incelendi" taklidi yapar ve EDevletDocument
/// yöntemini onaylar. documentRef içinde "reject" geçerse reddeder (test kolaylığı).
/// Üretimde bu sınıf, arayüzü koruyarak gerçek entegrasyonla değiştirilir.
/// </summary>
public sealed class MockVerificationService : IVerificationService
{
    public async Task<VerificationOutcome> ReviewAsync(VerificationMethod method, string? documentRef, CancellationToken ct = default)
    {
        await Task.Delay(400, ct); // inceleme gecikmesi taklidi

        if (!string.IsNullOrEmpty(documentRef) &&
            documentRef.Contains("reject", StringComparison.OrdinalIgnoreCase))
            return new VerificationOutcome(VerificationStatus.Rejected, "Belge doğrulanamadı (demo).");

        return method switch
        {
            VerificationMethod.EDevletDocument =>
                new VerificationOutcome(VerificationStatus.Verified, "Barkodlu belge doğrulandı (demo)."),
            VerificationMethod.InstitutionalEmail =>
                new VerificationOutcome(VerificationStatus.Verified, "Kurumsal e-posta doğrulandı (demo)."),
            VerificationMethod.RegistryNumber =>
                new VerificationOutcome(VerificationStatus.Verified, "Sicil numarası doğrulandı (demo)."),
            _ => new VerificationOutcome(VerificationStatus.Pending, "Bilinmeyen yöntem.")
        };
    }
}
