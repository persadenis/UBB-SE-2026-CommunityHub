using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class DiscussionReactionConfiguration : IEntityTypeConfiguration<DiscussionReaction>
{
    public void Configure(EntityTypeBuilder<DiscussionReaction> e)
    {
        e.HasKey(dr => new { dr.DiscussionId, dr.UserId });

        e.HasOne(dr => dr.Discussion)
            .WithMany(d => d.Reactions)
            .HasForeignKey(dr => dr.DiscussionId)
            .OnDelete(DeleteBehavior.NoAction);

        e.HasOne(dr => dr.Author)
            .WithMany()
            .HasForeignKey(dr => dr.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);

        e.HasOne(dr => dr.Message)
            .WithMany(m => m.Reactions)
            .HasForeignKey(dr => dr.MessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
