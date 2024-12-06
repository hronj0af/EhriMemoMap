using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PacovPlace
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

    public virtual ICollection<PacovDocument> PacovDocuments { get; set; } = new List<PacovDocument>();

    public virtual ICollection<PacovEntitiesXPlace> PacovEntitiesXPlaces { get; set; } = new List<PacovEntitiesXPlace>();

    public virtual ICollection<PacovIncident> PacovIncidents { get; set; } = new List<PacovIncident>();
    public virtual ICollection<PacovPoi> PacovPois { get; set; } = new List<PacovPoi>();

    public virtual ICollection<PacovTransport> PacovTransportPlaceFromNavigations { get; set; } = new List<PacovTransport>();

    public virtual ICollection<PacovTransport> PacovTransportPlaceToNavigations { get; set; } = new List<PacovTransport>();

    public virtual ICollection<PacovNarrativeMapStopsXPlace> PacovNarrativeMapStopsXPlaces { get; set; } = [];
}
