using System;
using System.Collections.Generic;
using System.Globalization;
using EhriMemoMap.Data;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Models;

public partial class MapObjectForLeafletModel
{
    public MapObjectForLeafletModel(MapObject mapObject)
    {
        PlaceType = mapObject.PlaceType;
        Citizens = mapObject.Citizens;
        CitizensTotal = mapObject.CitizensTotal;
        Id = mapObject.Id;
        Guid = mapObject.PlaceType + "_" + mapObject.Id + "_" + mapObject.DateFrom?.ToString("yyyy-mm-dd");
        Label = CultureInfo.CurrentCulture == new CultureInfo("en") ? mapObject.LabelEn : mapObject.LabelCs;
        MapPoint = mapObject.MapPoint;
        MapPolygon = mapObject.MapPolygon;
    }

    public string? PlaceType { get; set; }

    public decimal? Citizens { get; set; }

    public decimal? CitizensTotal { get; set; }
    public int? Id { get; set; }

    public string? Guid { get; set; }

    public string? Label { get; set; }

    public string? MapPoint { get; set; }

    public string? MapPolygon { get; set; }
}
