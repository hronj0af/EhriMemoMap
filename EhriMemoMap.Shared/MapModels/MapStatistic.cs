using NetTopologySuite.Geometries;

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
}
