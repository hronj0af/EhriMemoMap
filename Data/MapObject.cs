using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Data;

[Keyless]
public partial class MapObject
{
    [Column("place_type")]
    public string? PlaceType { get; set; }

    [Column("citizens")]
    public decimal? Citizens { get; set; }

    [Column("citizens_total")]
    public decimal? CitizensTotal { get; set; }

    [Column("id")]
    public int? Id { get; set; }

    [Column("label_cs")]
    public string? LabelCs { get; set; }

    [Column("label_en")]
    public string? LabelEn { get; set; }

    [Column("geography_map_point", TypeName = "geography")]
    public Geometry? GeographyMapPoint { get; set; }

    [Column("geography_map_polygon")]
    public Geometry? GeographyMapPolygon { get; set; }

    [Column("map_point")]
    public string? MapPoint { get; set; }

    [Column("map_polygon")]
    public string? MapPolygon { get; set; }

    [Column("date_from")]
    public DateTime? DateFrom { get; set; }

    [Column("date_to")]
    public DateTime? DateTo { get; set; }
}
