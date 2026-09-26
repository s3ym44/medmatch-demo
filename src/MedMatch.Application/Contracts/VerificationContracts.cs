using MedMatch.Domain.Enums;

namespace MedMatch.Application.Contracts;

public sealed record SubmitVerificationRequest(VerificationMethod Method, string? DocumentRef);
public sealed record VerificationDto(VerificationStatus Status, VerificationMethod Method, DateTimeOffset? ReviewedAt, string? Reason);
