using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Data.MemoMap;

public partial class MapObject
{
    public string? PlaceType { get; set; }

    public decimal? Citizens { get; set; }

    public decimal? CitizensTotal { get; set; }

    public long? Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public Geometry? GeographyMapPoint { get; set; }

    public Geometry? GeographyMapPolygon { get; set; }

    public string? MapPoint { get; set; }

    public string? MapPolygon { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}
