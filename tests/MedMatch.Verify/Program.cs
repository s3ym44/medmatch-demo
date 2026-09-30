using MedMatch.Application;
using MedMatch.Application.Abstractions;
using MedMatch.Application.Common;
using MedMatch.Application.Contracts;
using MedMatch.Application.Features.Auth;
using MedMatch.Application.Features.Chat;
using MedMatch.Application.Features.Discovery;
using MedMatch.Application.Features.Matching;
using MedMatch.Application.Features.Profiles;
using MedMatch.Application.Features.Verification;
using MedMatch.Infrastructure;
using MedMatch.Domain.Enums;
using MedMatch.Domain.Matching;
using MedMatch.Domain.Profiles;
using MedMatch.Domain.Common;
using MedMatch.Verify;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    new DateOnly(1995, 1, 1), "Ankara", Gender.Male, new AgeRange(25, 40),
    WorkSchedule.Shifts, NightShiftLoad.Moderate, MandatoryServiceStatus.Completed, RelocationOpenness.Open, CareerStage.Resident);
Check.True("Profil başlangıçta Unverified", prof.VerificationStatus == VerificationStatus.Unverified);
prof.MarkVerified();
Check.True("MarkVerified -> Verified", prof.VerificationStatus == VerificationStatus.Verified);
prof.AddPhoto("a.svg", false);
Check.True("İlk foto otomatik primary", prof.Photos.Single().IsPrimary);
prof.AddPhoto("b.svg", true);
Check.True("Yeni primary eklenince eski primary düşer",
    prof.Photos.Count(p => p.IsPrimary) == 1 && prof.Photos.First(p => p.Url == "b.svg").IsPrimary);

// Yeni takvim/coğrafya alanları
Check.True("Profil yeni alanları saklar",
    prof is { WorkSchedule: WorkSchedule.Shifts, NightShiftLoad: NightShiftLoad.Moderate,
              MandatoryService: MandatoryServiceStatus.Completed, Relocation: RelocationOpenness.Open,
              CareerStage: CareerStage.Resident });
Check.Throws<DomainException>("Tanımsız enum değeri fırlatır", () => DoctorProfile.Create(Guid.NewGuid(), "T", Profession.Physician,
    Gender.Female, new DateOnly(1995, 1, 1), "Ankara", Gender.Male, new AgeRange(25, 40),
    (WorkSchedule)0, NightShiftLoad.None, MandatoryServiceStatus.Completed, RelocationOpenness.Open, CareerStage.Resident));

// Prompt kuralları
Check.True("Prompt başlangıçta boş", prof.Prompts.Count == 0);
prof.AddPrompt(PromptKey.FreeWeekend, "  Dağa kaçarım.  ");
Check.True("Prompt eklenir ve kırpılır", prof.Prompts.Single().Answer == "Dağa kaçarım.");
Check.Throws<DomainException>("Aynı PromptKey iki kez eklenemez", () => prof.AddPrompt(PromptKey.FreeWeekend, "başka"));
Check.Throws<DomainException>("Boş cevap fırlatır", () => prof.AddPrompt(PromptKey.TusWinDay, "   "));
Check.Throws<DomainException>("201 karakter fırlatır", () => prof.AddPrompt(PromptKey.TusWinDay, new string('a', 201)));
prof.AddPrompt(PromptKey.TusWinDay, new string('a', 200));
Check.True("200 karakter kabul edilir", prof.Prompts.Count == 2);
prof.AddPrompt(PromptKey.HowToLoseMe, "x");
Check.Throws<DomainException>("4. prompt fırlatır", () => prof.AddPrompt(PromptKey.OffDutyDifferent, "y"));
Check.True("Katalog tüm PromptKey'leri kapsar",
    Enum.GetValues<PromptKey>().All(k => PromptCatalog.Texts.ContainsKey(k)));

Console.WriteLine();
Console.WriteLine("== Uçtan uca akış (in-memory, MediatR pipeline) ==");

// Uygulamanın gerçek DI kurulumu: AddApplication + AddInfrastructure (InMemory mod)
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Database:Provider"] = "InMemory",
    ["Jwt:Secret"] = "medmatch-verify-test-secret-at-least-32-bytes"
}).Build();
var services = new ServiceCollection().AddApplication().AddInfrastructure(config);
var uow = new RecordingUnitOfWork();
services.AddSingleton<IUnitOfWork>(uow); // transaction davranışını gözlemlemek için
await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
await using var scope = provider.CreateAsyncScope();
var sender = scope.ServiceProvider.GetRequiredService<ISender>();

async Task<bool> FailsWith(AppErrorType type, Func<Task> action)
{
    try { await action(); return false; }
    catch (AppException ex) { return ex.Type == type; }
}

// iki kullanıcı kaydol + profil + doğrula
var a = await sender.Send(new RegisterCommand(new RegisterRequest("a@x.dev", "parola1")));
var b = await sender.Send(new RegisterCommand(new RegisterRequest("b@x.dev", "parola1")));
await sender.Send(new CreateProfileCommand(a.UserId, new CreateProfileRequest("Ada", Profession.Physician, Gender.Female, new DateOnly(1994,1,1), "Ankara", null, Gender.Male, 25, 45, WorkSchedule.Daytime, NightShiftLoad.None, MandatoryServiceStatus.Completed, RelocationOpenness.Open, CareerStage.Specialist,
    [new PromptAnswerInput(PromptKey.FreeWeekend, "Dağ yürüyüşü.")])));
await sender.Send(new CreateProfileCommand(b.UserId, new CreateProfileRequest("Bora", Profession.Dentist, Gender.Male, new DateOnly(1990,1,1), "Ankara", null, Gender.Female, 25, 45, WorkSchedule.Shifts, NightShiftLoad.Heavy, MandatoryServiceStatus.Pending, RelocationOpenness.Closed, CareerStage.Resident)));

// doğrulama öncesi keşif engelli
Check.True("Doğrulanmadan keşif engelli",
    await FailsWith(AppErrorType.Forbidden, () => sender.Send(new GetCandidatesQuery(a.UserId, 10))));

await sender.Send(new SubmitVerificationCommand(a.UserId, new SubmitVerificationRequest(VerificationMethod.EDevletDocument, "belge")));
await sender.Send(new SubmitVerificationCommand(b.UserId, new SubmitVerificationRequest(VerificationMethod.EDevletDocument, "belge")));

var cands = await sender.Send(new GetCandidatesQuery(a.UserId, 10));
Check.True("Doğrulama sonrası Ada, Bora'yı görüyor", cands.Any(c => c.UserId == b.UserId));
var boraCard = cands.Single(c => c.UserId == b.UserId);
Check.True("Aday kartı yeni alanları taşır",
    boraCard is { WorkSchedule: WorkSchedule.Shifts, NightShiftLoad: NightShiftLoad.Heavy, Relocation: RelocationOpenness.Closed, CareerStage: CareerStage.Resident });
var adaProfile = await sender.Send(new GetMyProfileQuery(a.UserId));
Check.True("ProfileDto prompt metnini katalogdan getirir",
    adaProfile!.Prompts is [{ PromptKey: PromptKey.FreeWeekend, Answer: "Dağ yürüyüşü." } p0] && p0.PromptText == PromptCatalog.TextOf(PromptKey.FreeWeekend));

// tek taraflı like -> eşleşme yok
var r1 = await sender.Send(new SwipeCommand(a.UserId, new SwipeRequest(b.UserId, SwipeDecision.Like)));
Check.True("Tek taraflı like eşleşme üretmez", !r1.Matched);
// karşılıklı like -> eşleşme
var r2 = await sender.Send(new SwipeCommand(b.UserId, new SwipeRequest(a.UserId, SwipeDecision.Like)));
Check.True("Karşılıklı like eşleşme üretir", r2.Matched && r2.MatchId is not null);

// mesajlaşma + yetki
await sender.Send(new SendMessageCommand(a.UserId, r2.MatchId!.Value, new SendMessageRequest("Selam Bora")));
var msgs = await sender.Send(new GetMessagesQuery(b.UserId, r2.MatchId!.Value));
Check.True("Mesaj karşı tarafça görülüyor", msgs.Any(x => x.Content == "Selam Bora"));

var c = await sender.Send(new RegisterCommand(new RegisterRequest("c@x.dev", "parola1")));
Check.True("Üçüncü kişi sohbete erişemez",
    await FailsWith(AppErrorType.Forbidden, () => sender.Send(new GetMessagesQuery(c.UserId, r2.MatchId!.Value))));

Console.WriteLine();
Console.WriteLine("== Pipeline davranışları ==");

// Validation: elle yazılmış kontrollerle aynı mesaj ve 400 (Validation)
uow.Reset();
Check.True("Validation: geçersiz e-posta reddedilir",
    await FailsWith(AppErrorType.Validation, () => sender.Send(new RegisterCommand(new RegisterRequest("gecersiz", "parola1")))));
Check.True("Validation: kısa parola reddedilir",
    await FailsWith(AppErrorType.Validation, () => sender.Send(new RegisterCommand(new RegisterRequest("d@x.dev", "123")))));
Check.True("Validation: kendine oy reddedilir",
    await FailsWith(AppErrorType.Validation, () => sender.Send(new SwipeCommand(a.UserId, new SwipeRequest(a.UserId, SwipeDecision.Like)))));
Check.True("Validation transaction'dan önce: geçersiz istek transaction açmaz", uow.Calls == 0);

// Transaction: command'lar IUnitOfWork'ten geçer, query'ler geçmez
uow.Reset();
await sender.Send(new GetMatchesQuery(a.UserId));
await sender.Send(new GetMyProfileQuery(a.UserId));
Check.True("Transaction: query'ler transaction açmaz", uow.Calls == 0);
await sender.Send(new SendMessageCommand(b.UserId, r2.MatchId!.Value, new SendMessageRequest("Selam Ada")));
Check.True("Transaction: command tek transaction'da çalışır", uow.Calls == 1);

return Check.Summary();

/// <summary>Transaction davranışını gözlemlemek için: çağrıları sayar, işi doğrudan çalıştırır.</summary>
sealed class RecordingUnitOfWork : IUnitOfWork
{
    public int Calls { get; private set; }
    public void Reset() => Calls = 0;
    public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default)
    {
        Calls++;
        return work();
    }
}
