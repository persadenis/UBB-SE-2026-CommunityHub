using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class AnnouncementReactionConfiguration : IEntityTypeConfiguration<AnnouncementReaction>
{
    public void Configure(EntityTypeBuilder<AnnouncementReaction> e)
    {
        e.HasKey(ar => new { ar.AnnouncementId, ar.AuthorId });

        e.HasOne(ar => ar.Announcement)
            .WithMany(a => a.Reactions)
            .HasForeignKey(ar => ar.AnnouncementId)
            .OnDelete(DeleteBehavior.NoAction);

        e.HasOne(ar => ar.Author)
            .WithMany()
            .HasForeignKey(ar => ar.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}