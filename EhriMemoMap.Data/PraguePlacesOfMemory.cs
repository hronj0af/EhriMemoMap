using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

[Table("prague_places_of_memory")]
public partial class PraguePlacesOfMemory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("label")]
    public string? Label { get; set; }

    [Column("address_cs")]
    public string? AddressCs { get; set; }

    [Column("address_en")]
    public string? AddressEn { get; set; }

    [Column("link_stolpersteine_cs")]
    public string? LinkStolpersteinCs { get; set; }

    [Column("link_stolpersteine_en")]
    public string? LinkStolpersteinEn { get; set; }

    [Column("link_holocaust_cs")]
    public string? LinkHolocaustCs { get; set; }

    [Column("link_holocaust_en")]
    public string? LinkHolocaustEn { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }
}
