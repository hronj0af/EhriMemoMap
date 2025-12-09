using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data.MemoMap;

public partial class PlacesOfMemory
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? InscriptionCs { get; set; }

    public string? InscriptionEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public DateTime? CreationDate { get; set; }

    public virtual ICollection<DocumentsXPlacesOfMemory> DocumentsXPlacesOfMemories { get; set; } = new List<DocumentsXPlacesOfMemory>();

    public virtual ICollection<PlacesOfMemoryXPlacesOfMemory> PlacesOfMemoryXPlacesOfMemoryPlaceOfMemory1s { get; set; } = new List<PlacesOfMemoryXPlacesOfMemory>();

    public virtual ICollection<PlacesOfMemoryXPlacesOfMemory> PlacesOfMemoryXPlacesOfMemoryPlaceOfMemory2s { get; set; } = new List<PlacesOfMemoryXPlacesOfMemory>();

    public virtual ICollection<PlacesXPlacesOfMemory> PlacesXPlacesOfMemories { get; set; } = new List<PlacesXPlacesOfMemory>();
}
