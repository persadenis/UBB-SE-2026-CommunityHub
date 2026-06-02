using System;
using System.Collections.Generic;
using System.Text;

namespace ChatAndEvents.Data.EventsData.Models;

public class QuestMemory
{
    public required Quest ForQuest { get; set; }
    public required Memory Proof { get; set; }
    public QuestMemoryStatus ProofStatus { get; set; } = QuestMemoryStatus.Submitted;
    public int QuestId { get; set; }
    public int MemoryId { get; set; }

}
