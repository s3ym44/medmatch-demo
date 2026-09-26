namespace MedMatch.Domain.Common;

/// <summary>Bir domain kuralı ihlal edildiğinde fırlatılır.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
