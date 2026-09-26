namespace MedMatch.Domain.Enums;

/// <summary>
/// Kanıt yöntemleri. Demo'da yalnızca <see cref="EDevletDocument"/> mock akıştan geçer;
/// diğerleri soyutlamanın genişleyebilirliğini gösterir, tetiklenmez.
/// </summary>
public enum VerificationMethod
{
    EDevletDocument = 1,
    InstitutionalEmail = 2,
    RegistryNumber = 3
}
