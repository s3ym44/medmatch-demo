namespace MedMatch.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Secret { get; set; } = "medmatch-demo-super-secret-key-change-in-production-please";
    public string Issuer { get; set; } = "medmatch";
    public int LifetimeHours { get; set; } = 24;
}
