namespace MedMatch.Application.Abstractions;

public interface ITokenService
{
    /// <summary>Kullanıcı için imzalı erişim token'ı üretir.</summary>
    (string token, DateTimeOffset expiresAt) Issue(Guid userId, string email);

    /// <summary>Token'ı doğrular; geçerliyse userId döner, değilse null.</summary>
    Guid? Validate(string token);
}
