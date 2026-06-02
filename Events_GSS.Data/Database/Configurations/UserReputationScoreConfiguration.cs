using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class UserReputationScoreConfiguration : IEntityTypeConfiguration<UserReputationScore>
{
    public void Configure(EntityTypeBuilder<UserReputationScore> e)
    {
        e.ToTable("users_RP_scores");

        e.HasKey(r => r.UserId);

        e.Property(r => r.ReputationPoints)
            .HasDefaultValue(0)
            .IsRequired();

        e.Property(r => r.Tier)
            .HasMaxLength(50)
            .HasDefaultValue("Newcomer")
            .IsRequired();

        e.HasOne(r => r.User)
            .WithOne(u => u.ReputationScore)
            .HasForeignKey<UserReputationScore>(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
