using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class MemoryLikeConfiguration : IEntityTypeConfiguration<MemoryLike>
{
    public void Configure(EntityTypeBuilder<MemoryLike> e)
    {
        e.ToTable("MemoryLikes");

        e.HasKey(ml => new { ml.MemoryId, ml.UserId });

        e.HasOne(ml => ml.Memory)
            .WithMany(m => m.Likes)
            .HasForeignKey(ml => ml.MemoryId)
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(ml => ml.User)
            .WithMany()
            .HasForeignKey(ml => ml.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
