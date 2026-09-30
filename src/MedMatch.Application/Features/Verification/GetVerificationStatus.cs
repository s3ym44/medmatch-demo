using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MediatR;

namespace MedMatch.Application.Features.Verification;

public sealed record GetVerificationStatusQuery(Guid UserId) : IQuery<VerificationDto?>;

internal sealed class GetVerificationStatusHandler : IRequestHandler<GetVerificationStatusQuery, VerificationDto?>
{
    private readonly IVerificationRepository _requests;
    public GetVerificationStatusHandler(IVerificationRepository requests) => _requests = requests;

    public async Task<VerificationDto?> Handle(GetVerificationStatusQuery q, CancellationToken ct)
    {
        var request = await _requests.GetLatestForUserAsync(q.UserId, ct);
        return request is null ? null : new VerificationDto(request.Status, request.Method, request.ReviewedAt, null);
    }
}
