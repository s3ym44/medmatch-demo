# MedMatch

Yalnızca **doğrulanmış hekim ve diş hekimleri** için tanışma uygulaması — çalışan demo.

Ürünün asıl meselesi uygulama değil, **doğrulama güvenidir**: "buradaki herkes gerçekten hekim"
vaadi kurulamazsa geriye sıradan bir dating app kalır. Bu yüzden doğrulama bir özellik değil,
mimarinin merkezinde bir soyutlamadır.

- **Backend:** .NET 8, Clean Architecture (Domain / Application / Infrastructure / Api)
- **Frontend:** React 18 + TypeScript + Vite
- **Gerçek zamanlı sohbet:** SignalR
- **Çekirdek akış:** kayıt → profil → meslek doğrulama → keşif/swipe → eşleşme → canlı sohbet

---

## Hızlı başlangıç

İki terminal gerekir. Ön koşul: **.NET 8 SDK**, **Node 18+** ve **Docker**.

**1) Backend**
```bash
docker compose up -d
dotnet run --project src/MedMatch.Api --urls http://127.0.0.1:5099
```

API açılışta migration'ları uygular ve demo verisini yükler. Postgres host'ta **5433** portunda.
Docker olmadan denemek için in-memory mod (Redis backplane kapalı):
`Database__Provider=InMemory ConnectionStrings__Redis= dotnet run --project src/MedMatch.Api --urls http://127.0.0.1:5099`

Birden fazla API örneği SignalR mesajlarını Redis backplane üzerinden paylaşır (`ConnectionStrings:Redis`; boşsa tek sunuculu).

**2) Frontend**
```bash
cd web
npm install
npm run dev
```

Tarayıcıda `http://localhost:5173` → **"Demo hesabıyla tek tıkla gir"**.

Demo hesabı: `demo@medmatch.dev` / `demo1234` (doğrulanmış). Diğer tüm seed doktorların parolası da `demo1234`.

---

## Demo senaryosu (60 saniye)

1. **Tek tıkla gir** — Şeyma (Ankara, dahiliye, doğrulanmış) olarak açılır.
2. **Keşfet** — yalnızca doğrulanmış, tercihlere (cinsiyet/yaş) uyan doktorlar gelir.
3. **Kerem'i beğen** — Kerem seni önceden beğenmişti → **anında eşleşme** modalı.
4. **Eşleşmeler → Elif** — hazır bir sohbet geçmişi var.
5. İki sekmede aç, birinde yaz → diğerinde **canlı** düşer (SignalR).
6. Yeni bir hesap aç: doğrulanmadan **Keşfet 403** verir; "e-Devlet belge" gönderince açılır.
   (`documentRef` alanına `reject` yazarsan reddedilme yolunu görürsün.)

---

## Mimari

```
src/
  MedMatch.Domain          # Saf iş kuralları — dış bağımlılık yok
    Users, Profiles, Verification, Matching, Messaging
  MedMatch.Application      # Use-case'ler (MediatR command/query + handler), arayüzler, DTO'lar
    Abstractions, Features, Behaviors (validation, transaction), Contracts
  MedMatch.Infrastructure   # Arayüzlerin implementasyonu
    Persistence (EF Core + PostgreSQL; in-memory alternatif), Security (JWT/hash), Verification (mock), Seed
  MedMatch.Api              # Controller'lar, SignalR hub, auth, DI
tests/
  MedMatch.Verify           # Bağımlılıksız doğrulama harness'i (18 test)
web/                        # React + TS + Vite istemci
```

Bağımlılık yönü tek taraflı: `Api → Infrastructure → Application → Domain`. Domain hiçbir şeye bağlı değil.

### Doğrulama soyutlaması (ürünün kalbi)

`IVerificationService` tek bir arayüzdür; demo'da `MockVerificationService` "belgeyi aldım, onayladım"
der. Gerçek üründe bu arayüzün arkasına **e-Devlet barkodlu belge / kurumsal e-posta / oda sicili**
gibi yöntemler strateji olarak takılır — üst katmanların hiçbiri değişmez. `VerificationRequest`
entity'si bilinçli olarak sağlayıcıya özgü hiçbir alan tutmaz; yalnızca opak bir belge referansı ve durum.

> **Neden tek doğrulama yolu yok?** TTB/TDB üyeliği zorunlu olmadığından sicil no'yu tek kapı yapmak
> hedef kitlenin büyük kısmını dışarıda bırakır. Bu yüzden `VerificationMethod` çoklu kanıt için
> tasarlandı. Ayrıca "e-Devlet ile giriş" doğrudan entegrasyonu freelancer'a/girişime açık değildir;
> gerçekçi yol, kullanıcının e-Devlet'ten aldığı **barkodlu belgeyi** doğrulamaktır.

### Keşif ve gizlilik

`GetCandidatesHandler` tüm dışlama kurallarını tek yerde toplar: kendini, daha önce oy verdiklerini,
**doğrulanmamışları**, tercih dışı cinsiyet/yaşı eler. İleride "aynı kurumdaki meslektaşı / hastayı
görme" gibi kurallar için `PrivacyFilter` kancası bırakılmıştır (demo'da pasif).

---

## Testler

```bash
dotnet run --project tests/MedMatch.Verify -c Release
```

Domain guard'ları (Match kanonik sıra, self-swipe, yaş aralığı, foto primary mantığı) ve uçtan uca
akışı (doğrulama kapısı, karşılıklı like → eşleşme, mesaj yetkisi) kapsar — 18 test.

---

## Bu demonun bilinçli sadeleştirmeleri

Bu sürüm şu üretim bileşenlerini soyutlama arkasında ikame eder (persistence EF Core + PostgreSQL'e taşındı, bkz. [docs/MIGRATION.md](docs/MIGRATION.md)):

| Konu | Demo | Üretim yolu |
|------|------|-------------|
| Meslek doğrulama | `MockVerificationService` | `IVerificationService` arkasında gerçek e-Devlet/e-posta/sicil |
| Auth | JwtBearer, tek HS256 anahtar (`Jwt__Secret`; Development'ta `appsettings.Development.json`) | Döndürülen anahtar / secret store |
| Doğrulama testi | Console harness | xUnit + FluentAssertions |

Repository ve servis arayüzleri sabit olduğu için bu geçişler üst katmanları etkilemez.

> **Not:** İlk demo dış NuGet paketleri olmadan üretilmişti. Bu bileşenlerin üretim karşılıklarına
> adım adım geçiş [docs/MIGRATION.md](docs/MIGRATION.md) içinde; persistence geçişi tamamlandı.
