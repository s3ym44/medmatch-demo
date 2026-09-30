using MedMatch.Domain.Matching;
using MedMatch.Domain.Messaging;
using MedMatch.Domain.Profiles;
using MedMatch.Domain.Users;
using MedMatch.Domain.Verification;
using Microsoft.EntityFrameworkCore;

namespace MedMatch.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DoctorProfile> Profiles => Set<DoctorProfile>();
    public DbSet<ProfilePhoto> Photos => Set<ProfilePhoto>();
    public DbSet<ProfilePrompt> Prompts => Set<ProfilePrompt>();
    public DbSet<VerificationRequest> Verifications => Set<VerificationRequest>();
    public DbSet<Swipe> Swipes => Set<Swipe>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
