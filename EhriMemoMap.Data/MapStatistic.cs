using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class MapStatistic
{
    [Key]
    [Column("id")]
    public long? Id { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("quarter_cs")]
    public string? QuarterCs { get; set; }

    [Column("quarter_en")]
    public string? QuarterEn { get; set; }

    [Column("count")]
    public decimal? Count { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }

    [Column("geography_polygon")]
    public Geometry? GeographyPolygon { get; set; }

    [Column("map_polygon")]
    public string? MapPolygon { get; set; }

    [Column("map_point")]
    public string? MapPoint { get; set; }

    [Column("date_from")]
    public DateTime? DateFrom { get; set; }

    [Column("date_to")]
    public DateTime? DateTo { get; set; }

}
