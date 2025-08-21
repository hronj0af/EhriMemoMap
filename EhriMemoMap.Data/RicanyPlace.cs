using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class RicanyPlace
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

    public virtual ICollection<RicanyDocument> RicanyDocuments { get; set; } = new List<RicanyDocument>();

    public virtual ICollection<RicanyEntitiesXPlace> RicanyEntitiesXPlaces { get; set; } = new List<RicanyEntitiesXPlace>();

    public virtual ICollection<RicanyIncident> RicanyIncidents { get; set; } = new List<RicanyIncident>();
    public virtual ICollection<RicanyPoi> RicanyPois { get; set; } = new List<RicanyPoi>();

    public virtual ICollection<RicanyTransport> RicanyTransportPlaceFromNavigations { get; set; } = new List<RicanyTransport>();

    public virtual ICollection<RicanyTransport> RicanyTransportPlaceToNavigations { get; set; } = new List<RicanyTransport>();

    public virtual ICollection<RicanyNarrativeMapStopsXPlace> RicanyNarrativeMapStopsXPlaces { get; set; } = [];

    public virtual ICollection<RicanyEventsXPlace> RicanyEventsXPlaces { get; set; } = new List<RicanyEventsXPlace>();

    public virtual ICollection<RicanyPlacesXPlaceOfMemory> RicanyPlacesXPlacesOfMemory { get; set; } = new List<RicanyPlacesXPlaceOfMemory>();
}
