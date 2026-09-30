using MedMatch.Application.Contracts;
using MedMatch.Domain.Messaging;
using MedMatch.Domain.Profiles;

namespace MedMatch.Application.Common;

internal static class Mapping
{
    public static PhotoDto ToDto(this ProfilePhoto p) => new(p.Id, p.Url, p.IsPrimary, p.Order);

    public static ProfileDto ToDto(this DoctorProfile p, DateOnly today) => new(
        p.Id, p.UserId, p.DisplayName, p.Profession, p.Gender, p.AgeOn(today), p.City, p.Bio,
        p.VerificationStatus, p.InterestedIn, p.AgeRange.Min, p.AgeRange.Max,
        p.Photos.OrderBy(x => x.Order).Select(x => x.ToDto()).ToList());

    public static MessageDto ToDto(this Message m) =>
        new(m.Id, m.MatchId, m.SenderId, m.Content, m.SentAt, m.ReadAt);

    public static string? PrimaryPhotoUrl(this DoctorProfile p) =>
        p.Photos.FirstOrDefault(x => x.IsPrimary)?.Url ?? p.Photos.OrderBy(x => x.Order).FirstOrDefault()?.Url;
}
