using System;
using System.Collections.Generic;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class MemoryItemWebViewModel
{
    public Memory Memory { get; set; } = null!;
    public bool CanDelete { get; set; }
    public bool CanLike { get; set; }
}

public class MemoryViewModel
{
    public int EventId { get; set; }
    public List<MemoryItemWebViewModel> Memories { get; set; } = new();
    public List<string> GalleryPhotos { get; set; } = new();
    public bool ShowGallery { get; set; }
    public bool ShowOnlyMine { get; set; }
    public bool SortAscending { get; set; }
    public string? ErrorMessage { get; set; }
}