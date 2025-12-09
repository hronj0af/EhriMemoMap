using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class Document
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public int? OmekaId { get; set; }

    public string? OmekaIdno { get; set; }

    public string? OmekaUrl { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public string? CreationDateCs { get; set; }

    public string? CreationDateEn { get; set; }

    public long? CreationPlace { get; set; }

    public string? Owner { get; set; }

    public virtual Place? CreationPlaceNavigation { get; set; }

    public virtual ICollection<DocumentsXEntity> DocumentsXEntities { get; set; } = new List<DocumentsXEntity>();

    public virtual ICollection<DocumentsXEvent> DocumentsXEvents { get; set; } = new List<DocumentsXEvent>();

    public virtual ICollection<DocumentsXMedium> DocumentsXMedia { get; set; } = new List<DocumentsXMedium>();

    public virtual ICollection<DocumentsXNarrativeMapStop> DocumentsXNarrativeMapStops { get; set; } = new List<DocumentsXNarrativeMapStop>();

    public virtual ICollection<DocumentsXPlacesOfMemory> DocumentsXPlacesOfMemories { get; set; } = new List<DocumentsXPlacesOfMemory>();

    public virtual ICollection<DocumentsXPoi> DocumentsXPois { get; set; } = new List<DocumentsXPoi>();
}
