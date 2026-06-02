using System;
using System.Collections.Generic;
using System.Text;

namespace ChatAndEvents.Data.EventsData.Models;

public class Quest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; }= "";
    public int Difficulty { get; set; }= 3;
    public Quest? PrerequisiteQuest { get; set; } = null;
    public Event Event { get; set; }
    public int EventId { get; set; }
    public ICollection<QuestMemory> QuestMemories { get; set; }
}

