using FluentValidation;
using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Profiles;
using MediatR;

namespace MedMatch.Application.Features.Profiles;

public sealed record CreateProfileCommand(Guid UserId, CreateProfileRequest Request) : ICommand<ProfileDto>;

internal sealed class CreateProfileValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileValidator()
    {
        RuleFor(x => x.Request.WorkSchedule).IsInEnum().WithMessage("Geçerli bir çalışma düzeni seçin.");
        RuleFor(x => x.Request.NightShiftLoad).IsInEnum().WithMessage("Geçerli bir nöbet yoğunluğu seçin.");
        RuleFor(x => x.Request.MandatoryService).IsInEnum().WithMessage("Geçerli bir mecburi hizmet durumu seçin.");
        RuleFor(x => x.Request.Relocation).IsInEnum().WithMessage("Geçerli bir tayin açıklığı seçin.");
        RuleFor(x => x.Request.CareerStage).IsInEnum().WithMessage("Geçerli bir kariyer aşaması seçin.");

        RuleFor(x => x.Request.Prompts)
            .Must(p => p is null || p.Count <= DoctorProfile.MaxPrompts)
            .WithMessage($"En fazla {DoctorProfile.MaxPrompts} prompt seçebilirsiniz.")
            .Must(p => p is null || p.Select(x => x.PromptKey).Distinct().Count() == p.Count)
            .WithMessage("Aynı prompt birden fazla seçilemez.");

        RuleForEach(x => x.Request.Prompts).ChildRules(prompt =>
        {
            prompt.RuleFor(p => p.PromptKey).IsInEnum().WithMessage("Geçersiz prompt seçimi.");
            prompt.RuleFor(p => p.Answer)
                .Must(a => !string.IsNullOrWhiteSpace(a)).WithMessage("Prompt cevabı boş olamaz.")
                .Must(a => a is null || a.Trim().Length <= ProfilePrompt.MaxAnswerLength)
                .WithMessage($"Prompt cevabı en fazla {ProfilePrompt.MaxAnswerLength} karakter olabilir.");
        });
    }
}

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
