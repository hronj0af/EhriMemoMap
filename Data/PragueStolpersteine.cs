using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

[Table("prague_stolpersteine")]
public partial class PragueStolpersteine
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("label")]
    public string? Label { get; set; }

    [Column("address_cs")]
    public string? AddressCs { get; set; }

    [Column("address_en")]
    public string? AddressEn { get; set; }

    [Column("link_stolp_cs")]
    public string? LinkStolpCs { get; set; }

    [Column("link_stolp_en")]
    public string? LinkStolpEn { get; set; }

    [Column("link_holo_cs")]
    public string? LinkHoloCs { get; set; }

    [Column("link_holo_en")]
    public string? LinkHoloEn { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }
}
