using MedMatch.Domain.Common;

namespace MedMatch.Domain.Profiles;

/// <summary>Yaş aralığı değer nesnesi. 18 <= Min <= Max <= 99.</summary>
public sealed record AgeRange
{
    public int Min { get; }
    public int Max { get; }

    public AgeRange(int min, int max)
    {
        if (min < 18 || max > 99 || min > max)
            throw new DomainException($"Geçersiz yaş aralığı: [{min}, {max}]. 18 <= Min <= Max <= 99 olmalı.");
        Min = min;
        Max = max;
    }
}
