using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class AnnouncementReadReceiptConfiguration : IEntityTypeConfiguration<AnnouncementReadReceipt>
{
    public void Configure(EntityTypeBuilder<AnnouncementReadReceipt> e)
    {
        e.HasKey(arr => new { arr.AnnouncementId, arr.UserId });

        e.HasOne(arr => arr.Announcement)
            .WithMany(a => a.ReadReceipts)
            .HasForeignKey(arr => arr.AnnouncementId);

        e.HasOne(arr => arr.User)
            .WithMany()
            .HasForeignKey(arr => arr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}