namespace EhriMemoMap.Data.Ricany;

public partial class MapStatistic
{
    public Shared.MapStatistic ConvertToSharedStatistic()
    {
        return new Shared.MapStatistic
        {
            Id = Id,
            Type = Type,
            QuarterCs = QuarterCs,
            QuarterEn = QuarterEn,
            Count = Count,
            Geography = Geography,
            GeographyPolygon = GeographyPolygon,
            MapPolygon = MapPolygon,
            MapPoint = MapPoint,
            DateFrom = DateFrom,
            DateTo = DateTo
        };
    }
}
