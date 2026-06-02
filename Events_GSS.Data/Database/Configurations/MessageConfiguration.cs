using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.Database.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> e)
    {
        e.HasKey(m => m.Id);

        e.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(m => m.ReplyTo)
            .WithMany()
            .HasForeignKey(m => m.ReplyToId)
            .OnDelete(DeleteBehavior.NoAction);

        e.HasOne(m => m.ParentMessage)
            .WithMany()
            .HasForeignKey(m => m.ParentMessageId)
            .OnDelete(DeleteBehavior.NoAction);

        e.Property(m => m.Content)
            .HasMaxLength(1024);

        e.Property(m => m.AttachmentUrl)
            .HasMaxLength(500);

        e.Property(m => m.LinkPreviewTitle)
            .HasMaxLength(255);

        e.Property(m => m.LinkPreviewDesc)
            .HasMaxLength(500);

        e.HasIndex(m => new { m.ConversationId, m.CreatedAt });

        e.HasIndex(m => m.ParentMessageId);
    }
}