using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Profiles;
using MediatR;

namespace MedMatch.Application.Features.Profiles;

public sealed record CreateProfileCommand(Guid UserId, CreateProfileRequest Request) : ICommand<ProfileDto>;

internal sealed class CreateProfileHandler : IRequestHandler<CreateProfileCommand, ProfileDto>
{
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;
    public CreateProfileHandler(IProfileRepository profiles, IClock clock) { _profiles = profiles; _clock = clock; }

    public async Task<ProfileDto> Handle(CreateProfileCommand cmd, CancellationToken ct)
    {
        if (await _profiles.GetByUserIdAsync(cmd.UserId, ct) is not null)
            throw AppException.Conflict("Profil zaten var. Güncelleme uç noktasını kullanın.");

        var req = cmd.Request;
        var profile = DoctorProfile.Create(
            cmd.UserId, req.DisplayName, req.Profession, req.Gender, req.BirthDate,
            req.City, req.InterestedIn, new AgeRange(req.AgeMin, req.AgeMax),
            req.WorkSchedule, req.NightShiftLoad, req.MandatoryService, req.Relocation, req.CareerStage);
        profile.UpdateBio(req.Bio);
        foreach (var prompt in req.Prompts ?? [])
            profile.AddPrompt(prompt.PromptKey, prompt.Answer);

        await _profiles.AddAsync(profile, ct);
        return profile.ToDto(_clock.Today);
    }
}
