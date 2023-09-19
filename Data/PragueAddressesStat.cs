using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PragueAddressesStat
{
    public int Id { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public string? AddressDe { get; set; }

    public decimal? Count { get; set; }

    public decimal? Murdered { get; set; }

    public decimal? Survived { get; set; }

    public decimal? FateUnknown { get; set; }

    public decimal? Present19420101 { get; set; }

    public decimal? Present19430101 { get; set; }

    public decimal? Present19440101 { get; set; }

    public decimal? Present19450101 { get; set; }

    public decimal? Present19450509 { get; set; }

    public Geometry? Geography { get; set; }

    public virtual ICollection<PragueVictim> PragueVictims { get; } = new List<PragueVictim>();
}
