using MedMatch.Domain.Verification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedMatch.Infrastructure.Persistence.Configurations;

public sealed class VerificationRequestConfiguration : IEntityTypeConfiguration<VerificationRequest>
{
    public void Configure(EntityTypeBuilder<VerificationRequest> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Method).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => x.UserId);
    }
}
