using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Verification;

namespace MedMatch.Application.Services;

/// <summary>
/// Doğrulama akışını yönetir: talep oluşturur, sağlayıcıdan (mock) sonucu alır,
/// profildeki denormalize durumu tek yerden günceller.
/// </summary>
public sealed class VerificationAppService
{
    private readonly IVerificationRepository _requests;
    private readonly IProfileRepository _profiles;
    private readonly IVerificationService _provider;
    private readonly IClock _clock;

    public VerificationAppService(IVerificationRepository requests, IProfileRepository profiles, IVerificationService provider, IClock clock)
    {
        _requests = requests; _profiles = profiles; _provider = provider; _clock = clock;
    }

    public async Task<VerificationDto> SubmitAsync(Guid userId, SubmitVerificationRequest req, CancellationToken ct = default)
    {
        var profile = await _profiles.GetByUserIdAsync(userId, ct)
            ?? throw AppException.NotFound("Önce profil oluşturmalısınız.");

        var request = VerificationRequest.Create(userId, req.Method, req.DocumentRef, _clock.Now);
        profile.SetPending();
        await _requests.AddAsync(request, ct);
        await _profiles.UpdateAsync(profile, ct);

        var outcome = await _provider.ReviewAsync(req.Method, req.DocumentRef, ct);

        if (outcome.Status == VerificationStatus.Verified)
        {
            request.Approve(_clock.Now);
            profile.MarkVerified();
        }
        else if (outcome.Status == VerificationStatus.Rejected)
        {
            request.Reject(_clock.Now);
            profile.MarkRejected();
        }

        await _requests.UpdateAsync(request, ct);
        await _profiles.UpdateAsync(profile, ct);

        return new VerificationDto(request.Status, request.Method, request.ReviewedAt, outcome.Reason);
    }

    public async Task<VerificationDto?> GetStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var request = await _requests.GetLatestForUserAsync(userId, ct);
        return request is null ? null : new VerificationDto(request.Status, request.Method, request.ReviewedAt, null);
    }
}
