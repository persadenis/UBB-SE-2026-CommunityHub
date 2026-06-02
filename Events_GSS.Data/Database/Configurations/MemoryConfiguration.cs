using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class MemoryConfiguration : IEntityTypeConfiguration<Memory>
{
    public void Configure(EntityTypeBuilder<Memory> e)
    {
        e.ToTable("Memories");
        e.HasKey(m => m.MemoryId);

        e.Property(m => m.MemoryId)
            .ValueGeneratedOnAdd();

        e.Ignore(m => m.LikesCount);
        e.Ignore(m => m.IsLikedByCurrentUser);

        e.HasOne(m => m.Event)
            .WithMany(ev => ev.Memories)
            .HasForeignKey(m => m.EventId);

        e.HasOne(m => m.Author)
            .WithMany()
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
