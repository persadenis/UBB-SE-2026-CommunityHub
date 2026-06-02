using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAndEvents.Data.Database.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> e)
    {
        e.HasKey(p => p.Id);

        e.HasOne(p => p.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(p => p.LastReadMessage)
            .WithMany()
            .HasForeignKey(p => p.LastReadMessageId)
            .OnDelete(DeleteBehavior.NoAction);

        e.HasIndex(p => new { p.ConversationId, p.UserId })
            .IsUnique();

        e.HasIndex(p => p.UserId);

        e.HasIndex(p => p.ConversationId);

        e.Property(p => p.Nickname)
            .HasMaxLength(16);
    }
}