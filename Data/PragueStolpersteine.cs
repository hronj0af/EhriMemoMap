using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PragueStolpersteine
{
    public int Id { get; set; }

    public string? Label { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public string? LinkStolpCs { get; set; }

    public string? LinkStolpEn { get; set; }

    public string? LinkHoloCs { get; set; }

    public string? LinkHoloEn { get; set; }

    public Geometry? Geography { get; set; }
}
