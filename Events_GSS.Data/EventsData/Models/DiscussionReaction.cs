using System;
using System.Collections.Generic;
using System.Text;

namespace ChatAndEvents.Data.EventsData.Models;
public class DiscussionReaction
{
    public int Id { get; set; }
    public required string Emoji { get; set; }
    public DiscussionMessage Message { get; set; }
    public int MessageId { get; set; }
    public User Author { get; set; }
    public Guid AuthorId { get; set; }
    public int DiscussionId { get; set; }
    public Guid UserId { get; set; }
    public Discussion Discussion { get; internal set; }
}
