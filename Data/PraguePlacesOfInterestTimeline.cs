using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class PraguePlacesOfInterestTimeline
{
    public int? Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public string? Quarter1938 { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public bool? Inaccessible { get; set; }

    public Geometry? Geography { get; set; }

    public Geometry? GeographyPolygon { get; set; }

    public LocalDate? DateFrom { get; set; }

    public LocalDate? DateTo { get; set; }
}
