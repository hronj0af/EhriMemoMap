using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PragueAddressesStatsTimeline
{
    public int Id { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public string? AddressDe { get; set; }

    public short? Count { get; set; }

    public short? Present19411231 { get; set; }

    public short? Deported19411231 { get; set; }

    public float? Present19411231Perc { get; set; }

    public short? Present19420331 { get; set; }

    public short? Deported19420331 { get; set; }

    public float? Present19420331Perc { get; set; }

    public short? Present19420630 { get; set; }

    public short? Deported19420630 { get; set; }

    public float? Present19420630Perc { get; set; }

    public short? Present19420930 { get; set; }

    public short? Deported19420930 { get; set; }

    public float? Present19420930Perc { get; set; }

    public short? Present19421231 { get; set; }

    public short? Deported19421231 { get; set; }

    public float? Present19421231Perc { get; set; }

    public short? Present19430331 { get; set; }

    public short? Deported19430331 { get; set; }

    public float? Present19430331Perc { get; set; }

    public short? Present19430630 { get; set; }

    public short? Deported19430630 { get; set; }

    public float? Present19430630Perc { get; set; }

    public short? Present19430930 { get; set; }

    public short? Deported19430930 { get; set; }

    public float? Present19430930Perc { get; set; }

    public short? Present19431231 { get; set; }

    public short? Deported19431231 { get; set; }

    public float? Present19431231Perc { get; set; }

    public short? Present19440331 { get; set; }

    public short? Deported19440331 { get; set; }

    public float? Present19440331Perc { get; set; }

    public short? Present19440630 { get; set; }

    public short? Deported19440630 { get; set; }

    public float? Present19440630Perc { get; set; }

    public short? Present19440930 { get; set; }

    public short? Deported19440930 { get; set; }

    public float? Present19440930Perc { get; set; }

    public short? Present19441231 { get; set; }

    public short? Deported19441231 { get; set; }

    public float? Present19441231Perc { get; set; }

    public short? Present19450331 { get; set; }

    public short? Deported19450331 { get; set; }

    public float? Present19450331Perc { get; set; }

    public short? Present19450508 { get; set; }

    public short? Deported19450508 { get; set; }

    public float? Present19450508Perc { get; set; }

    public Geometry? Geography { get; set; }

    public short? Murdered { get; set; }

    public short? Survived { get; set; }

    public short? FateUnknown { get; set; }
}
