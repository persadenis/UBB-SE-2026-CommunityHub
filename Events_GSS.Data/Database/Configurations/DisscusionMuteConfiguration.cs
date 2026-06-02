using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class DiscussionMuteConfiguration : IEntityTypeConfiguration<DiscussionMute>
{
    public void Configure(EntityTypeBuilder<DiscussionMute> e)
    {
        e.HasKey(dm => new { dm.DiscussionId, dm.UserId });

        e.HasOne(dm => dm.Discussion)
            .WithMany(d => d.Mutes)
            .HasForeignKey(dm => dm.DiscussionId)
            .OnDelete(DeleteBehavior.NoAction);

        e.HasOne(dm => dm.MutedUser)
            .WithMany()
            .HasForeignKey(dm => dm.MutedUserId)
            .OnDelete(DeleteBehavior.NoAction);
        e.HasOne(dm=>dm.MutedBy)
            .WithMany()
            .HasForeignKey(dm => dm.MutedById)
            .OnDelete(DeleteBehavior.NoAction);
    }
}