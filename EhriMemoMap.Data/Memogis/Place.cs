using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data.Memogis;

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

}
