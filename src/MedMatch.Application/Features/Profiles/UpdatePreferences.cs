using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Profiles;
using MediatR;

namespace MedMatch.Application.Features.Profiles;

public sealed record UpdatePreferencesCommand(Guid UserId, UpdatePreferencesRequest Request) : ICommand<ProfileDto>;

internal sealed class UpdatePreferencesHandler : IRequestHandler<UpdatePreferencesCommand, ProfileDto>
{
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;
    public UpdatePreferencesHandler(IProfileRepository profiles, IClock clock) { _profiles = profiles; _clock = clock; }

    public async Task<ProfileDto> Handle(UpdatePreferencesCommand cmd, CancellationToken ct)
    {
        var profile = await _profiles.RequireByUserIdAsync(cmd.UserId, ct);
        profile.UpdatePreferences(cmd.Request.InterestedIn, new AgeRange(cmd.Request.AgeMin, cmd.Request.AgeMax));
        profile.UpdateBio(cmd.Request.Bio);
        await _profiles.UpdateAsync(profile, ct);
        return profile.ToDto(_clock.Today);
    }
}
