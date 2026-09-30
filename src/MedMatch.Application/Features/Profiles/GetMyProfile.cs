using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MediatR;

namespace MedMatch.Application.Features.Profiles;

public sealed record GetMyProfileQuery(Guid UserId) : IQuery<ProfileDto?>;

internal sealed class GetMyProfileHandler : IRequestHandler<GetMyProfileQuery, ProfileDto?>
{
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;
    public GetMyProfileHandler(IProfileRepository profiles, IClock clock) { _profiles = profiles; _clock = clock; }

    public async Task<ProfileDto?> Handle(GetMyProfileQuery q, CancellationToken ct)
        => (await _profiles.GetByUserIdAsync(q.UserId, ct))?.ToDto(_clock.Today);
}
