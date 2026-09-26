using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Domain.Profiles;

namespace MedMatch.Application.Services;

public sealed class ProfileService
{
    private readonly IProfileRepository _profiles;
    private readonly IClock _clock;

    public ProfileService(IProfileRepository profiles, IClock clock)
    {
        _profiles = profiles; _clock = clock;
    }

    public async Task<ProfileDto?> GetMineAsync(Guid userId, CancellationToken ct = default)
    {
        var p = await _profiles.GetByUserIdAsync(userId, ct);
        return p?.ToDto(_clock.Today);
    }

    public async Task<ProfileDto> CreateAsync(Guid userId, CreateProfileRequest req, CancellationToken ct = default)
    {
        if (await _profiles.GetByUserIdAsync(userId, ct) is not null)
            throw AppException.Conflict("Profil zaten var. Güncelleme uç noktasını kullanın.");

        var range = new AgeRange(req.AgeMin, req.AgeMax);
        var profile = DoctorProfile.Create(
            userId, req.DisplayName, req.Profession, req.Gender, req.BirthDate,
            req.City, req.InterestedIn, range);
        profile.UpdateBio(req.Bio);

        await _profiles.AddAsync(profile, ct);
        return profile.ToDto(_clock.Today);
    }

    public async Task<ProfileDto> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest req, CancellationToken ct = default)
    {
        var profile = await Require(userId, ct);
        profile.UpdatePreferences(req.InterestedIn, new AgeRange(req.AgeMin, req.AgeMax));
        profile.UpdateBio(req.Bio);
        await _profiles.UpdateAsync(profile, ct);
        return profile.ToDto(_clock.Today);
    }

    public async Task<ProfileDto> AddPhotoAsync(Guid userId, AddPhotoRequest req, CancellationToken ct = default)
    {
        var profile = await Require(userId, ct);
        profile.AddPhoto(req.Url, req.IsPrimary);
        await _profiles.UpdateAsync(profile, ct);
        return profile.ToDto(_clock.Today);
    }

    private async Task<DoctorProfile> Require(Guid userId, CancellationToken ct)
        => await _profiles.GetByUserIdAsync(userId, ct)
           ?? throw AppException.NotFound("Önce profil oluşturmalısınız.");
}
