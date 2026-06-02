using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAndEvents.Data.Database.Configurations;

public class FriendConfiguration : IEntityTypeConfiguration<Friend>
{
    public void Configure(EntityTypeBuilder<Friend> e)
    {
        e.HasKey(f => f.Id);

        e.HasOne(f => f.User1)
            .WithMany()
            .HasForeignKey(f => f.UserId1)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(f => f.User2)
            .WithMany()
            .HasForeignKey(f => f.UserId2)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasIndex(f => new { f.UserId1, f.UserId2 })
            .IsUnique();
    }
}