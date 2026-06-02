using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        e.HasKey(u => u.Id);

        e.Property(u => u.Id)
            .HasDefaultValueSql("NEWID()");

        e.Property(u => u.Username)
            .HasMaxLength(16)
            .IsRequired();

        e.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        e.Property(u => u.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        e.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        e.Property(u => u.Bio)
            .HasMaxLength(100);

        e.Property(u => u.Phone)
            .HasMaxLength(16);

        e.HasIndex(u => u.Username)
            .IsUnique();

        e.HasIndex(u => u.Email)
            .IsUnique();
    }
}  