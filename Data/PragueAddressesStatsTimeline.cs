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

    public short? Present19420101 { get; set; }

    public short? Deported19420101 { get; set; }

    public float? Present19420101Perc { get; set; }

    public short? Present19420401 { get; set; }

    public short? Deported19420401 { get; set; }

    public float? Present19420401Perc { get; set; }

    public short? Present19420701 { get; set; }

    public short? Deported19420701 { get; set; }

    public float? Present19420701Perc { get; set; }

    public short? Present19421001 { get; set; }

    public short? Deported19421001 { get; set; }

    public float? Present19421001Perc { get; set; }

    public short? Present19430101 { get; set; }

    public short? Deported19430101 { get; set; }

    public float? Present19430101Perc { get; set; }

    public short? Present19430401 { get; set; }

    public short? Deported19430401 { get; set; }

    public float? Present19430401Perc { get; set; }

    public short? Present19430701 { get; set; }

    public short? Deported19430701 { get; set; }

    public float? Present19430701Perc { get; set; }

    public short? Present19431001 { get; set; }

    public short? Deported19431001 { get; set; }

    public float? Present19431001Perc { get; set; }

    public short? Present19440101 { get; set; }

    public short? Deported19440101 { get; set; }

    public float? Present19440101Perc { get; set; }

    public short? Present19440401 { get; set; }

    public short? Deported19440401 { get; set; }

    public float? Present19440401Perc { get; set; }

    public short? Present19440701 { get; set; }

    public short? Deported19440701 { get; set; }

    public float? Present19440701Perc { get; set; }

    public short? Present19441001 { get; set; }

    public short? Deported19441001 { get; set; }

    public float? Present19441001Perc { get; set; }

    public short? Present19450101 { get; set; }

    public short? Deported19450101 { get; set; }

    public float? Present19450101Perc { get; set; }

    public short? Present19450401 { get; set; }

    public short? Deported19450401 { get; set; }

    public float? Present19450401Perc { get; set; }

    public short? Present19450508 { get; set; }

    public short? Deported19450508 { get; set; }

    public float? Present19450508Perc { get; set; }

    public Geometry? Geography { get; set; }

    public short? Murdered { get; set; }

    public short? Survived { get; set; }

    public short? FateUnknown { get; set; }
}
