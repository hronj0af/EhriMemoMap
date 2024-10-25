using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PraguePlacesOfMemory
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public string? Label { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public string? LinkStolpersteineCs { get; set; }

    public string? LinkStolpersteineEn { get; set; }

    public string? LinkHolocaustCs { get; set; }

    public string? LinkHolocaustEn { get; set; }

    public Geometry? Geography { get; set; }
}
