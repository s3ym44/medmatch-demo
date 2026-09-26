using MedMatch.Domain.Verification;

namespace MedMatch.Application.Abstractions;

public interface IVerificationRepository
{
    Task<VerificationRequest?> GetLatestForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(VerificationRequest request, CancellationToken ct = default);
    Task UpdateAsync(VerificationRequest request, CancellationToken ct = default);
}
