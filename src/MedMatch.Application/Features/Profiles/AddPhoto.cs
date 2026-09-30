using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MediatR;

namespace MedMatch.Application.Features.Profiles;

public sealed record AddPhotoCommand(Guid UserId, AddPhotoRequest Request) : ICommand<ProfileDto>;

internal sealed class AddPhotoHandler : IRequestHandler<AddPhotoCommand, ProfileDto>
{
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;
    public AddPhotoHandler(IProfileRepository profiles, IClock clock) { _profiles = profiles; _clock = clock; }

    public async Task<ProfileDto> Handle(AddPhotoCommand cmd, CancellationToken ct)
    {
        var profile = await _profiles.RequireByUserIdAsync(cmd.UserId, ct);
        profile.AddPhoto(cmd.Request.Url, cmd.Request.IsPrimary);
        await _profiles.UpdateAsync(profile, ct);
        return profile.ToDto(_clock.Today);
    }
}
