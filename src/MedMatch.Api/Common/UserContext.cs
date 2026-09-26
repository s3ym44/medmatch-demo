using System.Security.Claims;

namespace MedMatch.Api.Common;

public static class UserContext
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
