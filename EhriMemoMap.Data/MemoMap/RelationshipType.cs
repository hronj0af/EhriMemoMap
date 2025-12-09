using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class RelationshipType
{
    public long Id { get; set; }

    public string Table { get; set; } = null!;

    public string LabelCs { get; set; } = null!;

    public string LabelEn { get; set; } = null!;

    public virtual ICollection<EntitiesXEntity> EntitiesXEntities { get; set; } = new List<EntitiesXEntity>();

    public virtual ICollection<EntitiesXPlace> EntitiesXPlaces { get; set; } = new List<EntitiesXPlace>();

    public virtual ICollection<EventsXPlace> EventsXPlaces { get; set; } = new List<EventsXPlace>();

    public virtual ICollection<NarrativeMapStopsXPlace> NarrativeMapStopsXPlaces { get; set; } = new List<NarrativeMapStopsXPlace>();

    public virtual ICollection<PlacesOfMemoryXPlacesOfMemory> PlacesOfMemoryXPlacesOfMemories { get; set; } = new List<PlacesOfMemoryXPlacesOfMemory>();

    public virtual ICollection<PlacesXPlacesOfMemory> PlacesXPlacesOfMemories { get; set; } = new List<PlacesXPlacesOfMemory>();
}
