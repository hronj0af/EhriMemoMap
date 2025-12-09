using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data.MemoMap;

public partial class Place
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? TownCs { get; set; }

    public string? TownEn { get; set; }

    public string? StreetCs { get; set; }

    public string? StreetEn { get; set; }

    public string? HouseNr { get; set; }

    public string? RemarkCs { get; set; }

    public string? RemarkEn { get; set; }

    public Geometry? Geography { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<EntitiesXPlace> EntitiesXPlaces { get; set; } = new List<EntitiesXPlace>();

    public virtual ICollection<EventsXPlace> EventsXPlaces { get; set; } = new List<EventsXPlace>();

    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public virtual ICollection<NarrativeMapStopsXPlace> NarrativeMapStopsXPlaces { get; set; } = new List<NarrativeMapStopsXPlace>();

    public virtual ICollection<PlacesXPlacesOfMemory> PlacesXPlacesOfMemories { get; set; } = new List<PlacesXPlacesOfMemory>();

    public virtual ICollection<Poi> Pois { get; set; } = new List<Poi>();

    public virtual ICollection<Transport> TransportPlaceFromNavigations { get; set; } = new List<Transport>();

    public virtual ICollection<Transport> TransportPlaceToNavigations { get; set; } = new List<Transport>();
}
