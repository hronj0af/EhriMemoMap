using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Shared;

public partial class MapStatistic
{
    public long? Id { get; set; }

    public string? Type { get; set; }

    public string? QuarterCs { get; set; }

    public string? QuarterEn { get; set; }

    public decimal? Count { get; set; }

    public Geometry? Geography { get; set; }

    public Geometry? GeographyPolygon { get; set; }

    public string? MapPolygon { get; set; }

    public string? MapPoint { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public static MapStatistic ConvertFromMemogisStatistic(Data.MapStatistic mapStatistic)
    {
        return new MapStatistic
        {
            Id = mapStatistic.Id,
            Type = mapStatistic.Type,
            QuarterCs = mapStatistic.QuarterCs,
            QuarterEn = mapStatistic.QuarterEn,
            Count = mapStatistic.Count,
            Geography = mapStatistic.Geography,
            GeographyPolygon = mapStatistic.GeographyPolygon,
            MapPolygon = mapStatistic.MapPolygon,
            MapPoint = mapStatistic.MapPoint,
            DateFrom = mapStatistic.DateFrom,
            DateTo = mapStatistic.DateTo
        };
    }

    public static MapStatistic ConvertFromRicanyStatistic(Data.Ricany.MapStatistic ricanyStatistic)
    {
        return new MapStatistic
        {
            Id = ricanyStatistic.Id,
            Type = ricanyStatistic.Type,
            QuarterCs = ricanyStatistic.QuarterCs,
            QuarterEn = ricanyStatistic.QuarterEn,
            Count = ricanyStatistic.Count,
            Geography = ricanyStatistic.Geography,
            GeographyPolygon = ricanyStatistic.GeographyPolygon,
            MapPolygon = ricanyStatistic.MapPolygon,
            MapPoint = ricanyStatistic.MapPoint,
            DateFrom = ricanyStatistic.DateFrom,
            DateTo = ricanyStatistic.DateTo
        };
    }
}
