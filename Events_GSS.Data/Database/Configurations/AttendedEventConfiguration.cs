using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class AttendedEventConfiguration : IEntityTypeConfiguration<AttendedEvent>
{
    public void Configure(EntityTypeBuilder<AttendedEvent> e)
    {
        e.HasKey(ae => new { ae.EventId, ae.UserId });

        e.HasOne(ae => ae.Event)
            .WithMany(ev => ev.Attendees)
            .HasForeignKey(ae => ae.EventId);

        e.HasOne(ae => ae.User)
            .WithMany()
            .HasForeignKey(ae => ae.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}