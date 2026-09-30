using MedMatch.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedMatch.Infrastructure.Persistence.Configurations;

public sealed class ProfilePhotoConfiguration : IEntityTypeConfiguration<ProfilePhoto>
{
    public void Configure(EntityTypeBuilder<ProfilePhoto> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Url).IsRequired();
    }
}
