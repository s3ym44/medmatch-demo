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
    WorkSchedule WorkSchedule,
    NightShiftLoad NightShiftLoad,
    MandatoryServiceStatus MandatoryService,
    RelocationOpenness Relocation,
    CareerStage CareerStage,
    IReadOnlyList<PhotoDto> Photos,
    IReadOnlyList<PromptDto> Prompts);
