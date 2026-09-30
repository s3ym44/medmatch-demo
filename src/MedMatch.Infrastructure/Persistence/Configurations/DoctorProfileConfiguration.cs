using MedMatch.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedMatch.Infrastructure.Persistence.Configurations;

public sealed class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.UserId).IsUnique(); // 1-1 kullanıcı ilişkisi
        b.Property(x => x.DisplayName).IsRequired().HasMaxLength(120);
        b.Property(x => x.City).IsRequired().HasMaxLength(80);
        b.Property(x => x.VerificationStatus).HasConversion<int>();
        b.Property(x => x.WorkSchedule).HasConversion<int>();
        b.Property(x => x.NightShiftLoad).HasConversion<int>();
        b.Property(x => x.MandatoryService).HasConversion<int>();
        b.Property(x => x.Relocation).HasConversion<int>();
        b.Property(x => x.CareerStage).HasConversion<int>();

        // AgeRange value object -> AgeMin / AgeMax kolonları (ctor binding: min/max)
        b.OwnsOne(x => x.AgeRange, a =>
        {
            a.Property(p => p.Min).HasColumnName("AgeMin");
            a.Property(p => p.Max).HasColumnName("AgeMax");
        });
        b.Navigation(x => x.AgeRange).IsRequired();

        // Photos: salt-okunur koleksiyon, _photos backing field
        b.HasMany(x => x.Photos).WithOne().HasForeignKey(p => p.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Photos).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Prompts: Photos ile aynı desen (_prompts backing field)
        b.HasMany(x => x.Prompts).WithOne().HasForeignKey(p => p.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Prompts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
