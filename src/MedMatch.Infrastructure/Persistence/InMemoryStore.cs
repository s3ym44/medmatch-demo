using System.Collections.Concurrent;
using MedMatch.Domain.Matching;
using MedMatch.Domain.Messaging;
using MedMatch.Domain.Profiles;
using MedMatch.Domain.Users;
using MedMatch.Domain.Verification;

namespace MedMatch.Infrastructure.Persistence;

/// <summary>
/// Süreç içi (in-memory) veri deposu. Demo için tek örnek (singleton) olarak kullanılır.
/// Üretimde bu depo yerine EF Core + PostgreSQL implementasyonu geçer; repository arayüzleri
/// aynı kaldığı için üst katmanlar değişmez.
/// </summary>
public sealed class InMemoryStore
{
    public ConcurrentDictionary<Guid, User> Users { get; } = new();
    public ConcurrentDictionary<Guid, DoctorProfile> Profiles { get; } = new();
    public ConcurrentDictionary<Guid, VerificationRequest> Verifications { get; } = new();
    public ConcurrentDictionary<Guid, Swipe> Swipes { get; } = new();
    public ConcurrentDictionary<Guid, Match> Matches { get; } = new();
    public ConcurrentDictionary<Guid, Message> Messages { get; } = new();
}
