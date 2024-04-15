using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

[Table("prague_quarters_stats")]
public partial class PragueQuartersStat
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("type")]
    public string Type { get; set; } = null!;

    [Column("quarter_cs")]
    public string QuarterCs { get; set; } = null!;

    [Column("quarter_en")]
    public string QuarterEn { get; set; } = null!;

    [Column("count")]
    public decimal Count { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }

    [Column("geography_polygon")]
    public Geometry? GeographyPolygon { get; set; }
}
