using MedMatch.Domain.Common;
using MedMatch.Domain.Enums;

namespace MedMatch.Domain.Profiles;

/// <summary>Doktor profili. VerificationStatus keşif filtresi için denormalize tutulur.</summary>
public class DoctorProfile : Entity
{
    private readonly List<ProfilePhoto> _photos = new();

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

    public IReadOnlyList<ProfilePhoto> Photos => _photos.AsReadOnly();

    private DoctorProfile() { }

    public static DoctorProfile Create(
        Guid userId, string displayName, Profession profession, Gender gender,
        DateOnly birthDate, string city, Gender interestedIn, AgeRange ageRange)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId boş olamaz.");
        if (string.IsNullOrWhiteSpace(displayName)) throw new DomainException("Görünen ad boş olamaz.");
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("Şehir boş olamaz.");

        return new DoctorProfile
        {
            UserId = userId,
            DisplayName = displayName.Trim(),
            Profession = profession,
            Gender = gender,
            BirthDate = birthDate,
            City = city.Trim(),
            InterestedIn = interestedIn,
            AgeRange = ageRange
        };
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
}
