using MedMatch.Domain.Enums;

namespace MedMatch.Domain.Profiles;

/// <summary>Sunucu tanımlı prompt kataloğu (PromptKey -> Türkçe metin). Genişletmek için buraya satır ekle.</summary>
public static class PromptCatalog
{
    public static IReadOnlyDictionary<PromptKey, string> Texts { get; } = new Dictionary<PromptKey, string>
    {
        [PromptKey.NightShiftSurvival] = "Nöbette hayatta kalma taktiğim...",
        [PromptKey.CantTellPatients] = "Hastalarıma söyleyemediğim ama...",
        [PromptKey.TusWinDay] = "TUS'u kazandığım gün ilk yaptığım şey...",
        [PromptKey.IncompatibleSpecialty] = "Asla anlaşamayacağım branş...",
        [PromptKey.FreeWeekend] = "Boş bir hafta sonum olsa...",
        [PromptKey.HowToLoseMe] = "İlk buluşmada beni kaybetmenin yolu...",
        [PromptKey.OffDutyDifferent] = "Mesai dışında bambaşka biriyim çünkü..."
    };

    public static string TextOf(PromptKey key) =>
        Texts.TryGetValue(key, out var text) ? text : key.ToString();
}
