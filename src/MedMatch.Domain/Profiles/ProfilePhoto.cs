using MedMatch.Domain.Common;

namespace MedMatch.Domain.Profiles;

public class ProfilePhoto : Entity
{
    public Guid ProfileId { get; private set; }
    public string Url { get; private set; } = default!;
    public bool IsPrimary { get; private set; }
    public int Order { get; private set; }

    private ProfilePhoto() { }

    internal ProfilePhoto(Guid profileId, string url, bool isPrimary, int order)
    {
        ProfileId = profileId;
        Url = url;
        IsPrimary = isPrimary;
        Order = order;
    }

    internal void Demote() => IsPrimary = false;
}
