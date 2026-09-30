using MedMatch.Domain.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedMatch.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => new { x.UserAId, x.UserBId }).IsUnique();
        // Kanonik sıra (Match.Create) DB seviyesinde de garanti
        b.ToTable(t => t.HasCheckConstraint("CK_Match_Canonical", "\"UserAId\" < \"UserBId\""));
    }
}
