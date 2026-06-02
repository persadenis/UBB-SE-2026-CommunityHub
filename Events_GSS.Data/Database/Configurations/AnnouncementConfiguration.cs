using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> e)
    {
        e.HasKey(a => a.AnnouncementId);

        e.Property(a => a.AnnouncementId)
            .ValueGeneratedOnAdd();

        e.HasOne(a => a.Event)
            .WithMany(ev => ev.Announcements)
            .HasForeignKey(a => a.EventId);

        e.HasOne(a => a.Author)
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}