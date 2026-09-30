using MedMatch.Domain.Common;
using MedMatch.Domain.Enums;

namespace MedMatch.Domain.Profiles;

/// <summary>Doktor profili. VerificationStatus keşif filtresi için denormalize tutulur.</summary>
public class DoctorProfile : Entity
{
    public const int MaxPrompts = 3;

    private readonly List<ProfilePhoto> _photos = new();
    private readonly List<ProfilePrompt> _prompts = new();

    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = default!;
    public Profession Profession { get; private set; }
    public Gender Gender { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string City { get; private set; } = default!;
    public string? Bio { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Unverified;
    public Gender InterestedIn { get; private set; }
    public AgeRange AgeRange { get; private set; } = default!;
    public WorkSchedule WorkSchedule { get; private set; }
    public NightShiftLoad NightShiftLoad { get; private set; }
    public MandatoryServiceStatus MandatoryService { get; private set; }
    public RelocationOpenness Relocation { get; private set; }
    public CareerStage CareerStage { get; private set; }

    public IReadOnlyList<ProfilePhoto> Photos => _photos.AsReadOnly();
    public IReadOnlyList<ProfilePrompt> Prompts => _prompts.AsReadOnly();

    private DoctorProfile() { }

    public static DoctorProfile Create(
        Guid userId, string displayName, Profession profession, Gender gender,
        DateOnly birthDate, string city, Gender interestedIn, AgeRange ageRange,
        WorkSchedule workSchedule, NightShiftLoad nightShiftLoad, MandatoryServiceStatus mandatoryService,
        RelocationOpenness relocation, CareerStage careerStage)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId boş olamaz.");
        if (string.IsNullOrWhiteSpace(displayName)) throw new DomainException("Görünen ad boş olamaz.");
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("Şehir boş olamaz.");
        RequireDefined(workSchedule, "Çalışma düzeni");
        RequireDefined(nightShiftLoad, "Nöbet yoğunluğu");
        RequireDefined(mandatoryService, "Mecburi hizmet durumu");
        RequireDefined(relocation, "Tayin açıklığı");
        RequireDefined(careerStage, "Kariyer aşaması");

        return new DoctorProfile
        {
            UserId = userId,
            DisplayName = displayName.Trim(),
            Profession = profession,
            Gender = gender,
            BirthDate = birthDate,
            City = city.Trim(),
            InterestedIn = interestedIn,
            AgeRange = ageRange,
            WorkSchedule = workSchedule,
            NightShiftLoad = nightShiftLoad,
            MandatoryService = mandatoryService,
            Relocation = relocation,
            CareerStage = careerStage
        };
    }

    private static void RequireDefined<T>(T value, string label) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new DomainException($"{label} geçersiz.");
    }

    public int AgeOn(DateOnly today)
    {
        var age = today.Year - BirthDate.Year;
        if (BirthDate > today.AddYears(-age)) age--;
        return age;
    }

    public void UpdateBio(string? bio) => Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();

    public void UpdatePreferences(Gender interestedIn, AgeRange ageRange)
    {
        InterestedIn = interestedIn;
        AgeRange = ageRange;
    }

    public void SetPending() => VerificationStatus = VerificationStatus.Pending;
    public void MarkVerified() => VerificationStatus = VerificationStatus.Verified;
    public void MarkRejected() => VerificationStatus = VerificationStatus.Rejected;

    /// <summary>Foto ekler. İlk foto otomatik primary olur; yeni primary eklenirse eski primary düşer.</summary>
    public ProfilePhoto AddPhoto(string url, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new DomainException("Foto adresi boş olamaz.");

        var makePrimary = isPrimary || _photos.Count == 0;
        if (makePrimary)
            foreach (var existing in _photos)
                existing.Demote();

        var photo = new ProfilePhoto(Id, url.Trim(), makePrimary, _photos.Count);
        _photos.Add(photo);
        return photo;
    }

    /// <summary>Prompt cevabı ekler. En fazla 3 prompt, aynı anahtar bir kez, cevap boş değil ve en çok 200 karakter.</summary>
    public ProfilePrompt AddPrompt(PromptKey key, string answer)
    {
        if (!PromptCatalog.Texts.ContainsKey(key)) throw new DomainException("Bilinmeyen prompt.");
        if (string.IsNullOrWhiteSpace(answer)) throw new DomainException("Prompt cevabı boş olamaz.");
        var trimmed = answer.Trim();
        if (trimmed.Length > ProfilePrompt.MaxAnswerLength)
            throw new DomainException($"Prompt cevabı en fazla {ProfilePrompt.MaxAnswerLength} karakter olabilir.");
        if (_prompts.Count >= MaxPrompts)
            throw new DomainException($"En fazla {MaxPrompts} prompt seçilebilir.");
        if (_prompts.Any(p => p.PromptKey == key))
            throw new DomainException("Aynı prompt iki kez eklenemez.");

        var prompt = new ProfilePrompt(Id, key, trimmed, _prompts.Count);
        _prompts.Add(prompt);
        return prompt;
    }
}
