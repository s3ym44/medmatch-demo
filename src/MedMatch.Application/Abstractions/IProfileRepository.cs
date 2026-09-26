using MedMatch.Domain.Profiles;

namespace MedMatch.Application.Abstractions;

public interface IProfileRepository
{
    Task<DoctorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<DoctorProfile?> GetByIdAsync(Guid profileId, CancellationToken ct = default);
    Task<IReadOnlyList<DoctorProfile>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(DoctorProfile profile, CancellationToken ct = default);
    Task UpdateAsync(DoctorProfile profile, CancellationToken ct = default);
}
