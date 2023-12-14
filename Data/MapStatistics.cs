using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EhriMemoMap.Models;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class MapStatistics
{
    [Key]
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? QuarterCs { get; set; }
    public string? QuarterEn { get; set; }
    public decimal? Count { get; set; }
    public Geometry? Geography { get; set; }
    public Geometry? GeographyPolygon { get; set; }
    public string? MapPoint { get; set; }
    public string? MapPolygon { get; set; }

}
