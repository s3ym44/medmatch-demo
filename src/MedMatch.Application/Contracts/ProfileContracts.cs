using MedMatch.Domain.Enums;

namespace MedMatch.Application.Contracts;

public sealed record CreateProfileRequest(
    string DisplayName,
    Profession Profession,
    Gender Gender,
    DateOnly BirthDate,
    string City,
    string? Bio,
    Gender InterestedIn,
    int AgeMin,
    int AgeMax);

public sealed record UpdatePreferencesRequest(Gender InterestedIn, int AgeMin, int AgeMax, string? Bio);

public sealed record AddPhotoRequest(string Url, bool IsPrimary);

public sealed record PhotoDto(Guid Id, string Url, bool IsPrimary, int Order);

public sealed record ProfileDto(
    Guid Id,
    Guid UserId,
    string DisplayName,
    Profession Profession,
    Gender Gender,
    int Age,
    string City,
    string? Bio,
    VerificationStatus VerificationStatus,
    Gender InterestedIn,
    int AgeMin,
    int AgeMax,
    IReadOnlyList<PhotoDto> Photos);
