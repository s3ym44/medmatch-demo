using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Verification;
using MediatR;

namespace MedMatch.Application.Features.Verification;

/// <summary>
/// Talep oluşturur, sağlayıcıdan (mock) sonucu alır, profildeki denormalize durumu tek yerden günceller.
/// Talep ve profil güncellemesi tek transaction'da: yarım kalan "Pending" durumu oluşmaz.
/// </summary>
public sealed record SubmitVerificationCommand(Guid UserId, SubmitVerificationRequest Request) : ICommand<VerificationDto>;

internal sealed class SubmitVerificationHandler : IRequestHandler<SubmitVerificationCommand, VerificationDto>
{
    private readonly IVerificationRepository _requests;
    private readonly IProfileRepository _profiles;
    private readonly IVerificationService _provider;
    private readonly IClock _clock;

    public SubmitVerificationHandler(IVerificationRepository requests, IProfileRepository profiles, IVerificationService provider, IClock clock)
    {
        _requests = requests; _profiles = profiles; _provider = provider; _clock = clock;
    }

    public async Task<VerificationDto> Handle(SubmitVerificationCommand cmd, CancellationToken ct)
    {
        var req = cmd.Request;
        var profile = await _profiles.RequireByUserIdAsync(cmd.UserId, ct);

        var request = VerificationRequest.Create(cmd.UserId, req.Method, req.DocumentRef, _clock.Now);
        profile.SetPending();
        await _requests.AddAsync(request, ct);
        await _profiles.UpdateAsync(profile, ct);

        // Not: gerçek (yavaş, dış) sağlayıcı geldiğinde bu çağrı transaction'ı açık tutar;
        // o zaman akış "talep kaydet -> asenkron inceleme -> sonuç" olarak ikiye bölünmeli.
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
}
