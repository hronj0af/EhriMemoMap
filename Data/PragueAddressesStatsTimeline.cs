using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

[Table("prague_addresses_stats_timeline")]
public partial class PragueAddressesStatsTimeline
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("address_cs")]
    public string? AddressCs { get; set; }

    [Column("address_en")]
    public string? AddressEn { get; set; }

    [Column("address_de")]
    public string? AddressDe { get; set; }

    [Column("count")]
    public short? Count { get; set; }

    [Column("present_1942-01-01")]
    public short? Present19420101 { get; set; }

    [Column("deported_1942-01-01")]
    public short? Deported19420101 { get; set; }

    [Column("present_1942-01-01_perc")]
    public float? Present19420101Perc { get; set; }

    [Column("present_1942-04-01")]
    public short? Present19420401 { get; set; }

    [Column("deported_1942-04-01")]
    public short? Deported19420401 { get; set; }

    [Column("present_1942-04-01_perc")]
    public float? Present19420401Perc { get; set; }

    [Column("present_1942-07-01")]
    public short? Present19420701 { get; set; }

    [Column("deported_1942-07-01")]
    public short? Deported19420701 { get; set; }

    [Column("present_1942-07-01_perc")]
    public float? Present19420701Perc { get; set; }

    [Column("present_1942-10-01")]
    public short? Present19421001 { get; set; }

    [Column("deported_1942-10-01")]
    public short? Deported19421001 { get; set; }

    [Column("present_1942-10-01_perc")]
    public float? Present19421001Perc { get; set; }

    [Column("present_1943-01-01")]
    public short? Present19430101 { get; set; }

    [Column("deported_1943-01-01")]
    public short? Deported19430101 { get; set; }

    [Column("present_1943-01-01_perc")]
    public float? Present19430101Perc { get; set; }

    [Column("present_1943-04-01")]
    public short? Present19430401 { get; set; }

    [Column("deported_1943-04-01")]
    public short? Deported19430401 { get; set; }

    [Column("present_1943-04-01_perc")]
    public float? Present19430401Perc { get; set; }

    [Column("present_1943-07-01")]
    public short? Present19430701 { get; set; }

    [Column("deported_1943-07-01")]
    public short? Deported19430701 { get; set; }

    [Column("present_1943-07-01_perc")]
    public float? Present19430701Perc { get; set; }

    [Column("present_1943-10-01")]
    public short? Present19431001 { get; set; }

    [Column("deported_1943-10-01")]
    public short? Deported19431001 { get; set; }

    [Column("present_1943-10-01_perc")]
    public float? Present19431001Perc { get; set; }

    [Column("present_1944-01-01")]
    public short? Present19440101 { get; set; }

    [Column("deported_1944-01-01")]
    public short? Deported19440101 { get; set; }

    [Column("present_1944-01-01_perc")]
    public float? Present19440101Perc { get; set; }

    [Column("present_1944-04-01")]
    public short? Present19440401 { get; set; }

    [Column("deported_1944-04-01")]
    public short? Deported19440401 { get; set; }

    [Column("present_1944-04-01_perc")]
    public float? Present19440401Perc { get; set; }

    [Column("present_1944-07-01")]
    public short? Present19440701 { get; set; }

    [Column("deported_1944-07-01")]
    public short? Deported19440701 { get; set; }

    [Column("present_1944-07-01_perc")]
    public float? Present19440701Perc { get; set; }

    [Column("present_1944-10-01")]
    public short? Present19441001 { get; set; }

    [Column("deported_1944-10-01")]
    public short? Deported19441001 { get; set; }

    [Column("present_1944-10-01_perc")]
    public float? Present19441001Perc { get; set; }

    [Column("present_1945-01-01")]
    public short? Present19450101 { get; set; }

    [Column("deported_1945-01-01")]
    public short? Deported19450101 { get; set; }

    [Column("present_1945-01-01_perc")]
    public float? Present19450101Perc { get; set; }

    [Column("present_1945-04-01")]
    public short? Present19450401 { get; set; }

    [Column("deported_1945-04-01")]
    public short? Deported19450401 { get; set; }

    [Column("present_1945-04-01_perc")]
    public float? Present19450401Perc { get; set; }

    [Column("present_1945-05-08")]
    public short? Present19450508 { get; set; }

    [Column("deported_1945-05-08")]
    public short? Deported19450508 { get; set; }

    [Column("present_1945-05-08_perc")]
    public float? Present19450508Perc { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }

    [Column("murdered")]
    public short? Murdered { get; set; }

    [Column("survived")]
    public short? Survived { get; set; }

    [Column("fate_unknown")]
    public short? FateUnknown { get; set; }
}
