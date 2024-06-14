using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

[Table("prague_addresses_stats")]
[Index("Id", Name = "id", IsUnique = true)]
public partial class PragueAddressesStat
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

    [Column("address_current_cs")]
    public string? AddressCurrentCs { get; set; }

    [Column("address_current_en")]
    public string? AddressCurrentEn { get; set; }

    [Column("address_current_de")]
    public string? AddressCurrentDe { get; set; }

    [Column("count")]
    [Precision(5, 0)]
    public decimal? Count { get; set; }

    [Column("murdered")]
    [Precision(5, 0)]
    public decimal? Murdered { get; set; }

    [Column("survived")]
    [Precision(5, 0)]
    public decimal? Survived { get; set; }

    [Column("fate_unknown")]
    [Precision(5, 0)]
    public decimal? FateUnknown { get; set; }

    [Column("present_1942-01-01")]
    [Precision(5, 0)]
    public decimal? Present19420101 { get; set; }

    [Column("present_1943-01-01")]
    [Precision(5, 0)]
    public decimal? Present19430101 { get; set; }

    [Column("present_1944-01-01")]
    [Precision(5, 0)]
    public decimal? Present19440101 { get; set; }

    [Column("present_1945-01-01")]
    [Precision(5, 0)]
    public decimal? Present19450101 { get; set; }

    [Column("present_1945-05-09")]
    [Precision(5, 0)]
    public decimal? Present19450509 { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }

    [InverseProperty("Place")]
    public virtual ICollection<PragueVictim> PragueVictims { get; set; } = new List<PragueVictim>();
}
