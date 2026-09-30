using MedMatch.Application.Abstractions;
using MedMatch.Domain.Matching;
using MedMatch.Domain.Profiles;

namespace MedMatch.Application.Common;

/// <summary>Handler'larda tekrarlanan "bul ya da hata ver" kontrolleri.</summary>
internal static class RepositoryExtensions
{
    public static async Task<DoctorProfile> RequireByUserIdAsync(this IProfileRepository profiles, Guid userId, CancellationToken ct)
        => await profiles.GetByUserIdAsync(userId, ct)
           ?? throw AppException.NotFound("Önce profil oluşturmalısınız.");

    /// <summary>Eşleşmeyi getirir; kullanıcı bu eşleşmenin tarafı değilse 403.</summary>
    public static async Task<Match> RequireMembershipAsync(this IMatchRepository matches, Guid userId, Guid matchId, CancellationToken ct)
    {
        var match = await matches.GetByIdAsync(matchId, ct)
            ?? throw AppException.NotFound("Eşleşme bulunamadı.");
        if (!match.Involves(userId))
            throw AppException.Forbidden("Bu sohbete erişim yetkiniz yok.");
        return match;
    }
}
