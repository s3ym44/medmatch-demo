using MedMatch.Domain.Enums;

namespace MedMatch.Application.Abstractions;

/// <summary>
/// Meslek doğrulama sağlayıcısı soyutlaması. Demo'da mock implementasyon "belgeyi aldım,
/// onayladım" der. Gerçek entegrasyon (e-Devlet barkodlu belge, kurumsal e-posta, sicil no)
/// bu arayüzün arkasına takılır; üst katmanlar değişmez.
/// </summary>
public interface IVerificationService
{
    Task<VerificationOutcome> ReviewAsync(VerificationMethod method, string? documentRef, CancellationToken ct = default);
}

public sealed record VerificationOutcome(VerificationStatus Status, string? Reason);
