namespace EhriMemoMap.Data;

public partial class MapObject
{
    public Shared.MapObject ConvertToMapObjectShared()
    {
        return new Shared.MapObject
        {
            PlaceType = PlaceType,
            LayerName = PlaceType,
            Citizens = Citizens,
            CitizensTotal = CitizensTotal,
            Id = Id,
            LabelCs = LabelCs,
            LabelEn = LabelEn,
            GeographyMapPoint = GeographyMapPoint,
            GeographyMapPolygon = GeographyMapPolygon,
            MapPoint = MapPoint,
            MapPolygon = MapPolygon,
            DateFrom = DateFrom,
            DateTo = DateTo,
        };
    }
}
