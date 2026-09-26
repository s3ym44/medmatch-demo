using MedMatch.Domain.Common;

namespace MedMatch.Domain.Users;

/// <summary>Hesap/kimlik. Profilden ayrıdır: bir kullanıcı = bir hesap.</summary>
public class User : Entity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastActiveAt { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("E-posta boş olamaz.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Parola hash'i boş olamaz.");

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = now,
            LastActiveAt = now
        };
    }

    public void Touch(DateTimeOffset now) => LastActiveAt = now;
}
