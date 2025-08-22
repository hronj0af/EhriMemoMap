using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Shared;

public partial class MapObject
{
    public string? PlaceType { get; set; }

    public decimal? Citizens { get; set; }

    public decimal? CitizensTotal { get; set; }

    public long? Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public Geometry? GeographyMapPoint { get; set; }

    public Geometry? GeographyMapPolygon { get; set; }

    public string? MapPoint { get; set; }

    public string? MapPolygon { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }


    public static MapObject ConvertFromMemogisObject(Data.MapObject mapObject)
    {
        return new MapObject
        {
            PlaceType = mapObject.PlaceType,
            Citizens = mapObject.Citizens,
            CitizensTotal = mapObject.CitizensTotal,
            Id = mapObject.Id,
            LabelCs = mapObject.LabelCs,
            LabelEn = mapObject.LabelEn,
            GeographyMapPoint = mapObject.GeographyMapPoint,
            GeographyMapPolygon = mapObject.GeographyMapPolygon,
            MapPoint = mapObject.MapPoint,
            MapPolygon = mapObject.MapPolygon,
            DateFrom = mapObject.DateFrom,
            DateTo = mapObject.DateTo
        };
    }

    public static MapObject ConvertFromRicanyObject(Data.Ricany.MapObject ricanyObject)
    {
        return new MapObject
        {
            PlaceType = ricanyObject.PlaceType,
            Citizens = ricanyObject.Citizens,
            CitizensTotal = ricanyObject.CitizensTotal,
            Id = ricanyObject.Id,
            LabelCs = ricanyObject.LabelCs,
            LabelEn = ricanyObject.LabelEn,
            GeographyMapPoint = ricanyObject.GeographyMapPoint,
            GeographyMapPolygon = ricanyObject.GeographyMapPolygon,
            MapPoint = ricanyObject.MapPoint,
            MapPolygon = ricanyObject.MapPolygon,
            DateFrom = ricanyObject.DateFrom,
            DateTo = ricanyObject.DateTo
        };
    }
}
