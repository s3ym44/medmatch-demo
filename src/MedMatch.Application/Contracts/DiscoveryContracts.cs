using MedMatch.Domain.Enums;

namespace MedMatch.Application.Contracts;

public sealed record CandidateDto(
    Guid ProfileId,
    Guid UserId,
    string DisplayName,
    Profession Profession,
    Gender Gender,
    int Age,
    string City,
    string? Bio,
    IReadOnlyList<PhotoDto> Photos);
