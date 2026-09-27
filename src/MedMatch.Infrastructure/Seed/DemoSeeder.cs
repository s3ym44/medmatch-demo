using MedMatch.Application.Abstractions;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Matching;
using MedMatch.Domain.Messaging;
using MedMatch.Domain.Profiles;
using MedMatch.Domain.Users;

namespace MedMatch.Infrastructure.Seed;

/// <summary>
/// Demo verisi: giriş hesabı, doğrulanmış doktorlar, hazır bir eşleşme ve sohbet.
/// Yalnızca repository arayüzlerini kullanır; InMemory ve Postgres modunda aynı şekilde çalışır.
/// </summary>
public sealed class DemoSeeder
{
    public const string DemoEmail = "demo@medmatch.dev";
    public const string DemoPassword = "demo1234";

    private readonly IUserRepository _users;
    private readonly IProfileRepository _profiles;
    private readonly ISwipeRepository _swipes;
    private readonly IMatchRepository _matches;
    private readonly IMessageRepository _messages;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;

    // Önce hepsi bellekte kurulur, sonra sırayla yazılır (bkz. SeedAsync sonu)
    private readonly List<User> _pendingUsers = new();
    private readonly List<DoctorProfile> _pendingProfiles = new();
    private readonly List<Swipe> _pendingSwipes = new();
    private readonly List<Match> _pendingMatches = new();
    private readonly List<Message> _pendingMessages = new();

    public DemoSeeder(
        IUserRepository users, IProfileRepository profiles, ISwipeRepository swipes,
        IMatchRepository matches, IMessageRepository messages, IPasswordHasher hasher, IClock clock)
    {
        _users = users; _profiles = profiles; _swipes = swipes;
        _matches = matches; _messages = messages; _hasher = hasher; _clock = clock;
    }

    private static string Avatar(string seed) =>
        $"https://api.dicebear.com/7.x/avataaars/svg?seed={Uri.EscapeDataString(seed)}&backgroundColor=b6e3f4,c0aede,ffd5dc";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // idempotent: demo hesabı en son yazılır, varsa seed tamamlanmıştır
        if (await _users.GetByEmailAsync(DemoEmail, ct) is not null) return;

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
        _pendingMatches.Add(match);
        AddMessage(match.Id, elif.user.Id, "Selam! Profilinde dağ yürüyüşü yazıyor, en son nereye gittin?", now.AddDays(-1).AddHours(-2));
        AddMessage(match.Id, meUser.Id, "Merhaba! Geçen hafta Ilgaz'daydım, muhteşemdi. Sen seramikle ne yapıyorsun?", now.AddDays(-1).AddHours(-1));
        AddMessage(match.Id, elif.user.Id, "Kupa ve saksı ağırlıklı :) Bir ara atölyeye davet ederim.", now.AddDays(-1));

        // Yazma sırası: önce diğer kullanıcılar, demo hesabı en son. Seed yarıda kesilirse demo hesabı
        // olmaz ve sonraki açılış ilk kullanıcıda unique e-posta ihlaliyle düşer: sessizce eksik kalmaz,
        // yetim profil/swipe da çoğaltmaz. (Postgres'te düzeltmek için tabloları boşalt.)
        foreach (var u in _pendingUsers.Where(u => u.Id != meUser.Id)) await _users.AddAsync(u, ct);
        foreach (var p in _pendingProfiles) await _profiles.AddAsync(p, ct);
        foreach (var s in _pendingSwipes) await _swipes.AddAsync(s, ct);
        foreach (var m in _pendingMatches) await _matches.AddAsync(m, ct);
        foreach (var m in _pendingMessages) await _messages.AddAsync(m, ct);
        await _users.AddAsync(meUser, ct);
    }

    private (User user, DoctorProfile profile) MakeDoctor(
        string email, string password, string name, Profession prof, Gender gender,
        DateOnly birth, string city, string bio, Gender interestedIn, int ageMin, int ageMax, bool verified)
    {
        var user = User.Create(email, _hasher.Hash(password), _clock.Now);
        _pendingUsers.Add(user);

        var profile = DoctorProfile.Create(user.Id, name, prof, gender, birth, city, interestedIn, new AgeRange(ageMin, ageMax));
        profile.UpdateBio(bio);
        profile.AddPhoto(Avatar(name), true);
        if (verified) profile.MarkVerified();
        _pendingProfiles.Add(profile);

        return (user, profile);
    }

    private void AddSwipe(Guid swiper, Guid target, SwipeDecision decision, DateTimeOffset at)
        => _pendingSwipes.Add(Swipe.Create(swiper, target, decision, at));

    private void AddMessage(Guid matchId, Guid sender, string content, DateTimeOffset at)
        => _pendingMessages.Add(Message.Create(matchId, sender, content, at));
}
