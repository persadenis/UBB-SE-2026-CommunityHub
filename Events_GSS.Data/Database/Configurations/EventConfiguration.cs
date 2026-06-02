using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> e)
    {
        e.HasKey(ev => ev.EventId);

        e.Property(ev => ev.EventId)
            .ValueGeneratedOnAdd();

        e.HasOne(ev => ev.Admin)
            .WithMany()
            .HasForeignKey(ev => ev.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(ev => ev.Category)
            .WithMany()
            .HasForeignKey(ev => ev.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        e.Property(ev => ev.Name)
            .IsRequired();

        e.Property(ev => ev.Location)
            .IsRequired();

    }
}