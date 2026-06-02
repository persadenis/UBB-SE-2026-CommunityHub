using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAndEvents.Data.Database.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> e)
    {
        e.ToTable("Notifications");

        e.HasKey(n => n.Id);

        e.Property(n => n.Id)
            .ValueGeneratedOnAdd();

        e.Property(n => n.Title)
            .IsRequired();

        e.Property(n => n.Description)
            .IsRequired();

        e.Property(n => n.Type)
            .HasMaxLength(80)
            .IsRequired();

        e.Property(n => n.SourceFeature)
            .HasMaxLength(80)
            .IsRequired();

        e.Property(n => n.SourceEntityId)
            .HasMaxLength(120);

        e.Property(n => n.IsRead)
            .HasDefaultValue(false);

        e.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(n => n.UserId);
        e.HasIndex(n => new { n.UserId, n.IsRead });
    }
}
