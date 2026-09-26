using MedMatch.Application.Abstractions;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Matching;
using MedMatch.Domain.Messaging;
using MedMatch.Domain.Profiles;
using MedMatch.Domain.Users;
using MedMatch.Infrastructure.Persistence;

namespace MedMatch.Infrastructure.Seed;

/// <summary>Demo verisi: giriş hesabı, doğrulanmış doktorlar, hazır bir eşleşme ve sohbet.</summary>
public sealed class DemoSeeder
{
    public const string DemoEmail = "demo@medmatch.dev";
    public const string DemoPassword = "demo1234";

    private readonly InMemoryStore _store;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;

    public DemoSeeder(InMemoryStore store, IPasswordHasher hasher, IClock clock)
    {
        _store = store; _hasher = hasher; _clock = clock;
    }

    private static string Avatar(string seed) =>
        $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(seed)}&backgroundColor=b6e3f4,c0aede,ffd5dc";

    public void Seed()
    {
        if (!_store.Users.IsEmpty) return; // idempotent

        var now = _clock.Now;

        // --- Giriş yapılacak demo hesabı (doğrulanmış) ---
        var (meUser, meProfile) = MakeDoctor(
            DemoEmail, DemoPassword, "Şeyma", Profession.Physician, Gender.Female,
            new DateOnly(1996, 4, 12), "Ankara", "Dahiliye uzmanı. Kahve, dağ yürüyüşü, iyi kitap.",
            interestedIn: Gender.Male, ageMin: 28, ageMax: 40, verified: true);

        // --- Aday doktorlar (hepsi doğrulanmış) ---
        var kerem = MakeDoctor("kerem@medmatch.dev", "demo1234", "Kerem", Profession.Physician, Gender.Male,
            new DateOnly(1992, 8, 3), "Ankara", "Kardiyoloji. Bisiklet ve vinil plak koleksiyonu.",
            Gender.Female, 26, 38, true);
        var emre = MakeDoctor("emre@medmatch.dev", "demo1234", "Emre", Profession.Dentist, Gender.Male,
            new DateOnly(1990, 1, 22), "Ankara", "Diş hekimi, kendi kliniğim var. Yüzme ve espresso.",
            Gender.Female, 27, 39, true);
        var elif = MakeDoctor("elif@medmatch.dev", "demo1234", "Elif", Profession.Physician, Gender.Female,
            new DateOnly(1994, 11, 9), "İstanbul", "Çocuk doktoru. Yoga, seramik, kedi annesi.",
            Gender.Male, 28, 42, true);
        var mert = MakeDoctor("mert@medmatch.dev", "demo1234", "Mert", Profession.Physician, Gender.Male,
            new DateOnly(1988, 6, 15), "Ankara", "Ortopedi. Koşu, doğa kampı, fotoğraf.",
            Gender.Female, 25, 40, true);
        var canan = MakeDoctor("canan@medmatch.dev", "demo1234", "Canan", Profession.Dentist, Gender.Female,
            new DateOnly(1995, 3, 30), "İzmir", "Ortodonti. Deniz, gitar, İtalyan mutfağı.",
            Gender.Male, 29, 41, true);
        var deniz = MakeDoctor("deniz@medmatch.dev", "demo1234", "Deniz", Profession.Physician, Gender.Male,
            new DateOnly(1991, 9, 18), "Ankara", "Nöroloji. Satranç, caz, uzun yürüyüşler.",
            Gender.Female, 26, 38, true);
        var selin = MakeDoctor("selin@medmatch.dev", "demo1234", "Selin", Profession.Dentist, Gender.Female,
            new DateOnly(1997, 2, 7), "Ankara", "Diş hekimi. Pilates, resim, brunch avcısı.",
            Gender.Male, 28, 40, true);
        var burak = MakeDoctor("burak@medmatch.dev", "demo1234", "Burak", Profession.Physician, Gender.Male,
            new DateOnly(1989, 12, 1), "Bursa", "Göz hastalıkları. Tenis, seyahat, şarap.",
            Gender.Female, 27, 42, true);

        // --- Kerem seni önceden beğenmiş: onu beğenince ANINDA eşleşme olacak ---
        AddSwipe(kerem.user.Id, meUser.Id, SwipeDecision.Like, now.AddHours(-3));

        // --- Elif ile zaten eşleşme + sohbet (sohbet ekranı dolu gelsin) ---
        AddSwipe(meUser.Id, elif.user.Id, SwipeDecision.Like, now.AddDays(-2));
        AddSwipe(elif.user.Id, meUser.Id, SwipeDecision.Like, now.AddDays(-2).AddMinutes(10));
        var match = Match.Create(meUser.Id, elif.user.Id, now.AddDays(-2).AddMinutes(10));
        _store.Matches[match.Id] = match;
        AddMessage(match.Id, elif.user.Id, "Selam! Profilinde dağ yürüyüşü yazıyor, en son nereye gittin?", now.AddDays(-1).AddHours(-2));
        AddMessage(match.Id, meUser.Id, "Merhaba! Geçen hafta Ilgaz'daydım, muhteşemdi. Sen seramikle ne yapıyorsun?", now.AddDays(-1).AddHours(-1));
        AddMessage(match.Id, elif.user.Id, "Kupa ve saksı ağırlıklı :) Bir ara atölyeye davet ederim.", now.AddDays(-1));
    }

    private (User user, DoctorProfile profile) MakeDoctor(
        string email, string password, string name, Profession prof, Gender gender,
        DateOnly birth, string city, string bio, Gender interestedIn, int ageMin, int ageMax, bool verified)
    {
        var user = User.Create(email, _hasher.Hash(password), _clock.Now);
        _store.Users[user.Id] = user;

        var profile = DoctorProfile.Create(user.Id, name, prof, gender, birth, city, interestedIn, new AgeRange(ageMin, ageMax));
        profile.UpdateBio(bio);
        profile.AddPhoto(Avatar(name), true);
        if (verified) profile.MarkVerified();
        _store.Profiles[profile.Id] = profile;

        return (user, profile);
    }

    private void AddSwipe(Guid swiper, Guid target, SwipeDecision decision, DateTimeOffset at)
    {
        var s = Swipe.Create(swiper, target, decision, at);
        _store.Swipes[s.Id] = s;
    }

    private void AddMessage(Guid matchId, Guid sender, string content, DateTimeOffset at)
    {
        var m = Message.Create(matchId, sender, content, at);
        _store.Messages[m.Id] = m;
    }
}
