# Görev: Eşleşme sonrası "birlikte oyun" özelliği

> SIRA NOTU: Bu özellik MVP sonrası bir retention işidir. Gerçek doğrulama ve KVKK işlerinden
> SONRA sıraya alınmalı. Burada tasarımı hazır dursun; kod yazımına o ikisi bitmeden başlama.

Repo: medmatch-demo (main). Clean Architecture, MediatR (command/handler + FluentValidation),
EF Core + PostgreSQL, SignalR, React + TS. Mevcut konvansiyonlara birebir uy. Ayrı dalda çalış
(feat/match-games), bitince push et, merge etme.

## Amaç ve temel tasarım kararı

Eşleşmelerin çoğu "eşleştik ama kimse yazmadı" diye ölür. Oyun bu sessizliği kıran bir yeniden
bağlanma aracıdır, salt eğlence değil.

EN KRİTİK KISIT: Bu kitle (hekimler) nöbette saatlerce offline olur. Bu yüzden oyun ASENKRON
olmak zorunda. Format her modda aynı: "ikiniz de kendi cevabınızı verirsiniz, ikiniz de bitirince
karşılıklı açılır." Gerçek zamanlı-only bir oyun bu uygulamada ölür. SignalR yalnızca ikisi de
online olduğunda canlı bildirim için kullanılır; durum veritabanında tutulur ki async çalışsın.

## Modlar

1. **Zor Hasta Senaryosu (çekirdek).** Sunucu tanımlı bir senaryo ikisine de gösterilir; her biri
   "ben nasıl yaklaşırdım" diye kısa metin yazar. İkisi de yazınca cevaplar yan yana açılır.
   DOĞRU CEVAP YOK, puanlama yok. Amaç karakteri (sabır, empati, iletişim tarzı) göstermek.
2. **Aynı Fikirde Miyiz? (hafif).** Ikisi de aynı soru setini cevaplar (kimi sabit şıklı, kimi
   açık uçlu). İkisi de bitince açılır. İstenirse hafif bir "uyum %" gösterilebilir ama oyunbaz
   kalsın, ciddi test gibi olmasın.
3. **Birlikte Teşhis (FAZ 2, opsiyonel).** Kooperatif: bir vakayı yarışarak değil BİRLİKTE
   çözerler. Sıra tabanlı, daha karmaşık. İlk sürüme ALMA, sadece modeli buna uygun bırak.

## Domain (yeni alan: MedMatch.Domain.Games)

- GameMode { PatientScenario, CompatibilityQuiz, CooperativeDiagnosis }
- GameState { AwaitingResponses, Revealed, Abandoned }
- GameSession entity: Id, MatchId, Mode, ContentRef (hangi senaryo/soru seti), State, CreatedAt,
  RevealedAt?. Backing field ile iki PlayerResponse tutar.
- PlayerResponse: SessionId, UserId, Payload (metin ya da seçim JSON'u), SubmittedAt.
- Domain kuralları:
  - Bir session'a en fazla iki farklı UserId cevap verebilir; aynı kullanıcı iki kez veremez.
  - İki cevap da gelince State -> Revealed, RevealedAt set edilir (domain metodu SubmitResponse
    içinde karar verir).
  - Cevaplar yalnız Revealed durumunda dışarı açılır; öncesinde karşı tarafın payload'u DTO'ya konmaz.
- İçerik katalogları sunucu tanımlı, tek yerde static (tıpkı PromptCatalog gibi):
  - ScenarioCatalog: zor hasta senaryoları (Türkçe metin). En az 8 senaryo.
  - CompatibilityCatalog: uyum soruları (bazıları şıklı). En az 8 soru.

## API

- POST /api/matches/{matchId}/games        -> yeni oyun başlat (body: mode). GameSession oluşturur,
  rastgele/uygun içerik seçer, döndürür. (ICommand, transaction'lı)
- POST /api/games/{sessionId}/responses    -> kendi cevabını gönder. (ICommand)
- GET  /api/games/{sessionId}              -> durum + içerik + (Revealed ise) iki cevap. (IQuery)
- GET  /api/matches/{matchId}/games        -> bu eşleşmenin oyun geçmişi. (IQuery)
- GET  /api/games/catalog?mode=...         -> mod için içerik başlıkları (gerekirse). (IQuery)
- Yetki: yalnız eşleşmenin iki tarafı erişebilir. Mevcut RequireMembershipAsync desenini kullan.

## Gerçek zamanlı (SignalR)

Ayrı hub açma, mevcut ChatHub'ın eşleşme grubunu kullan. Yeni event'ler:
- "GameStarted" (karşı taraf oyun başlattı), "OpponentAnswered" (karşı taraf cevapladı),
  "GameRevealed" (ikisi de bitti, açıldı). Hepsi async durumun üstüne canlı bildirim, zorunluluk değil.

## Kilit açma (unlock)

- Faz 1: oyunu sohbet ekranından elle başlatma yeterli ("Birlikte oyna" butonu).
- Faz 2 (opsiyonel): eşleşip ~24 saat mesajlaşma olmazsa bir dürtme göster ("buzları oyunla kır").
  İlk sürüme şart değil.

## Web (React + TS)

- Sohbet ekranında "Birlikte oyna" girişi -> mod seç -> oyna.
- Üç ekran durumu: (1) sen cevaplarken, (2) "karşı taraf henüz cevaplamadı" bekleme, (3) açıldı,
  iki cevap yan yana.
- Açıldıktan sonra "sohbete taşı" ile sonucu konuşmaya düşürebilsin (retention'ın asıl amacı bu).
- Zor Hasta Senaryosu cevap kutusunda ince placeholder: "Hasta bilgisi paylaşmadan yaz."

## Kısıtlar ve riskler

- **Async zorunlu.** Hiçbir mod ikisinin de aynı anda online olmasını gerektirmesin.
- **Yalnız iki mod.** PatientScenario + CompatibilityQuiz. CooperativeDiagnosis faz 2, ilk sürüme alma.
- **Puanlama yok (senaryoda).** Karakter aynası, sınav değil. Rekabet dinamiğinden kaçın.
- **Mesajlaşmayı oyunun arkasına kilitleme.** Oyun yardımcı, duvar değil.
- **Mahremiyet.** Senaryolar sunucu yazımı (güvenli). Serbest metin cevaplar gerçek hasta bilgisi
  içerebilir: placeholder uyarısı + cevaplar yalnız eşleşen iki kişiye görünür, asla public değil.
- **Kapsam kayması.** Çizim/canvas, gerçek zamanlı yarış gibi şeylere girme.

## İzole commit planı

1. feat(domain): Games alanı (GameSession, PlayerResponse, enum'lar, kataloglar, reveal kuralları)
2. feat(infra): EF config + migration (+ demo için Elif eşleşmesine örnek tamamlanmış bir oyun seed'le)
3. feat(api): endpoint'ler + validator'lar + ChatHub game event'leri
4. feat(web): sohbet içi oyun akışı (başlat / bekle / açıldı / sohbete taşı)

Her adım kendi commit'i. Mevcut akışlara davranış değişikliği yok. Testleri güncelle/ekle, hepsi geçsin,
build 0 uyarı. Bitince özet çıkar ve dalı push et; merge öncesi diff'i inceleteceğiz.
