using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAndEvents.Data.Database.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> e)
    {
        e.HasKey(c => c.Id);

        e.HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(c => c.PinnedMessage)
            .WithMany()
            .HasForeignKey(c => c.PinnedMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        e.Property(c => c.Title)
            .HasMaxLength(100);

        e.Property(c => c.IconUrl)
            .HasMaxLength(500);
    }
}