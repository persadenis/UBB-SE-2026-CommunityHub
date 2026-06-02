using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.Database.Configurations;

public class QuestMemoryConfiguration : IEntityTypeConfiguration<QuestMemory>
{
    public void Configure(EntityTypeBuilder<QuestMemory> e)
    {
        e.HasKey(qm => new { qm.QuestId, qm.MemoryId });

        e.HasOne(qm => qm.ForQuest)
            .WithMany(q => q.QuestMemories)
            .HasForeignKey(qm => qm.QuestId)
            .OnDelete(DeleteBehavior.NoAction);

        e.HasOne(qm => qm.Proof)
            .WithMany()
            .HasForeignKey(qm => qm.MemoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
