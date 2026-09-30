using MedMatch.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedMatch.Infrastructure.Persistence.Configurations;

public sealed class ProfilePromptConfiguration : IEntityTypeConfiguration<ProfilePrompt>
{
    public void Configure(EntityTypeBuilder<ProfilePrompt> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.PromptKey).HasConversion<int>();
        b.Property(x => x.Answer).IsRequired().HasMaxLength(ProfilePrompt.MaxAnswerLength);
        b.HasIndex(x => new { x.ProfileId, x.PromptKey }).IsUnique(); // aynı prompt iki kez olmaz
    }
}
