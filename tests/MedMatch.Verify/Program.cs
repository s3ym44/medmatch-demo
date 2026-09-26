using MedMatch.Application.Contracts;
using MedMatch.Application.Services;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Matching;
using MedMatch.Domain.Profiles;
using MedMatch.Domain.Common;
using MedMatch.Infrastructure.Persistence;
using MedMatch.Infrastructure.Persistence.Repositories;
using MedMatch.Infrastructure.Security;
using MedMatch.Infrastructure.Verification;
using MedMatch.Verify;

Console.WriteLine("== Domain guard testleri ==");

// Match kanonik sıra: argüman sırasından bağımsız aynı sonuç
var x = Guid.NewGuid();
var y = Guid.NewGuid();
var m1 = Match.Create(x, y, DateTimeOffset.UtcNow);
var m2 = Match.Create(y, x, DateTimeOffset.UtcNow);
Check.True("Match kanonik sıra tutarlı", m1.UserAId == m2.UserAId && m1.UserBId == m2.UserBId);
Check.True("Match UserAId < UserBId", m1.UserAId.CompareTo(m1.UserBId) < 0);
Check.Throws<DomainException>("Match aynı id ile fırlatır", () => Match.Create(x, x, DateTimeOffset.UtcNow));

// Swipe self-swipe
Check.Throws<DomainException>("Swipe self-swipe fırlatır", () => Swipe.Create(x, x, SwipeDecision.Like, DateTimeOffset.UtcNow));

// AgeRange sınırlar
Check.Throws<DomainException>("AgeRange min>max fırlatır", () => new AgeRange(40, 30));
Check.Throws<DomainException>("AgeRange <18 fırlatır", () => new AgeRange(16, 30));
Check.Throws<DomainException>("AgeRange >99 fırlatır", () => new AgeRange(30, 120));
Check.True("AgeRange geçerli", new AgeRange(28, 40) is { Min: 28, Max: 40 });

// DoctorProfile: doğrulama + foto primary mantığı
var prof = DoctorProfile.Create(Guid.NewGuid(), "Test", Profession.Physician, Gender.Female,
    new DateOnly(1995, 1, 1), "Ankara", Gender.Male, new AgeRange(25, 40));
Check.True("Profil başlangıçta Unverified", prof.VerificationStatus == VerificationStatus.Unverified);
prof.MarkVerified();
Check.True("MarkVerified -> Verified", prof.VerificationStatus == VerificationStatus.Verified);
prof.AddPhoto("a.svg", false);
Check.True("İlk foto otomatik primary", prof.Photos.Single().IsPrimary);
prof.AddPhoto("b.svg", true);
Check.True("Yeni primary eklenince eski primary düşer",
    prof.Photos.Count(p => p.IsPrimary) == 1 && prof.Photos.First(p => p.Url == "b.svg").IsPrimary);

Console.WriteLine();
Console.WriteLine("== Uçtan uca akış (in-memory) ==");

var clock = new SystemClock();
var store = new InMemoryStore();
var users = new UserRepository(store);
var profiles = new ProfileRepository(store);
var swipes = new SwipeRepository(store);
var matches = new MatchRepository(store);
var messages = new MessageRepository(store);
var verifications = new VerificationRepository(store);
var hasher = new Pbkdf2PasswordHasher();
var tokens = new JwtTokenService(new JwtOptions(), clock);
var provider = new MockVerificationService();

var auth = new AuthService(users, profiles, hasher, tokens, clock);
var profileSvc = new ProfileService(profiles, clock);
var verifySvc = new VerificationAppService(verifications, profiles, provider, clock);
var discovery = new DiscoveryService(profiles, swipes, clock);
var matching = new MatchingService(swipes, matches, profiles, clock);
var chat = new ChatService(matches, messages, profiles, clock);

// iki kullanıcı kaydol + profil + doğrula
var a = await auth.RegisterAsync(new RegisterRequest("a@x.dev", "parola1"));
var b = await auth.RegisterAsync(new RegisterRequest("b@x.dev", "parola1"));
await profileSvc.CreateAsync(a.UserId, new CreateProfileRequest("Ada", Profession.Physician, Gender.Female, new DateOnly(1994,1,1), "Ankara", null, Gender.Male, 25, 45));
await profileSvc.CreateAsync(b.UserId, new CreateProfileRequest("Bora", Profession.Dentist, Gender.Male, new DateOnly(1990,1,1), "Ankara", null, Gender.Female, 25, 45));

// doğrulama öncesi keşif engelli
var blocked = false;
try { await discovery.GetCandidatesAsync(a.UserId, 10); }
catch (MedMatch.Application.Common.AppException) { blocked = true; }
Check.True("Doğrulanmadan keşif engelli", blocked);

await verifySvc.SubmitAsync(a.UserId, new SubmitVerificationRequest(VerificationMethod.EDevletDocument, "belge"));
await verifySvc.SubmitAsync(b.UserId, new SubmitVerificationRequest(VerificationMethod.EDevletDocument, "belge"));

var cands = await discovery.GetCandidatesAsync(a.UserId, 10);
Check.True("Doğrulama sonrası Ada, Bora'yı görüyor", cands.Any(c => c.UserId == b.UserId));

// tek taraflı like -> eşleşme yok
var r1 = await matching.SwipeAsync(a.UserId, new SwipeRequest(b.UserId, SwipeDecision.Like));
Check.True("Tek taraflı like eşleşme üretmez", !r1.Matched);
// karşılıklı like -> eşleşme
var r2 = await matching.SwipeAsync(b.UserId, new SwipeRequest(a.UserId, SwipeDecision.Like));
Check.True("Karşılıklı like eşleşme üretir", r2.Matched && r2.MatchId is not null);

// mesajlaşma + yetki
await chat.SendAsync(a.UserId, r2.MatchId!.Value, new SendMessageRequest("Selam Bora"));
var msgs = await chat.GetMessagesAsync(b.UserId, r2.MatchId!.Value);
Check.True("Mesaj karşı tarafça görülüyor", msgs.Any(x => x.Content == "Selam Bora"));

var c = await auth.RegisterAsync(new RegisterRequest("c@x.dev", "parola1"));
var forbidden = false;
try { await chat.GetMessagesAsync(c.UserId, r2.MatchId!.Value); }
catch (MedMatch.Application.Common.AppException) { forbidden = true; }
Check.True("Üçüncü kişi sohbete erişemez", forbidden);

return Check.Summary();
