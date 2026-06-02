
namespace ChatAndEvents.Data.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

public class DiscussionConfiguration : IEntityTypeConfiguration<Discussion>
{
    public void Configure(EntityTypeBuilder<Discussion> e)
    {
        e.HasKey(d => d.Id);

        e.Property(d => d.Id)
            .ValueGeneratedOnAdd();

        e.HasOne(d => d.AssociatedEvent)
            .WithMany()
            .HasForeignKey("EventId");

        e.HasOne(d => d.Creator)
            .WithMany()
            .HasForeignKey("CreatorId")
            .OnDelete(DeleteBehavior.Restrict);

        e.Ignore(d => d.Messages);
        e.Ignore(d => d.Reactions);
        e.Ignore(d => d.Mutes);
    }
}
