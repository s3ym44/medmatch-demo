namespace MedMatch.Domain.Enums;

/// <summary>Aylık nöbet yoğunluğu; bilinçli olarak kova (tam sayı değil).</summary>
public enum NightShiftLoad
{
    None = 1,     // Yok
    Light = 2,    // Az (1-3)
    Moderate = 3, // Orta (4-7)
    Heavy = 4     // Yoğun (8+)
}
