using System;
using System.Collections.Generic;
using System.Globalization;
using EhriMemoMap.Data;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using NodaTime;
using EhriMemoMap.Resources;
using Microsoft.Extensions.Localization;

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
        Label = CultureInfo.CurrentCulture.Name == "en-US" ? mapObject.LabelEn : mapObject.LabelCs;
        MapPoint = mapObject.MapPoint;
        MapPolygon = mapObject.MapPolygon;

        if (PlaceType == Models.PlaceType.Incident.ToString())
            HtmlIcon = "<img src='images/incident.png' height=23 width=23 style='opacity:1'/>";

        else if (PlaceType == Models.PlaceType.Interest.ToString())
            HtmlIcon = "<img src='images/interest.png' height=23 width=23 style='opacity:1'/>";

        else if (PlaceType == Models.PlaceType.Address.ToString())
        {
            var saturation = Citizens / CitizensTotal;
            HtmlIcon = "<div style='position:relative'><img src='images/address.png' height=23 width=23 style='opacity:1;filter:saturate(" + saturation + ")'/><span style='position:absolute;top:50%;left:80%;transform: translate(-50%, -50%);'>" + Citizens + "</span></div>";
        }
    }

    public MapObjectForLeafletModel(List<PragueQuartersStat> quarterStatistics, IStringLocalizer<CommonResources> cl)
    {
        if (quarterStatistics == null || quarterStatistics.Count == 0)
            return;

        PlaceType = Models.PlaceType.Statistics.ToString();
        Guid = PlaceType + "_" + Id + "_";

        var serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var victims = quarterStatistics.FirstOrDefault(a => a.Type.Contains("victims"))?.Count;
        var incidents = quarterStatistics.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count;
        var interests = quarterStatistics.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count;
        var inaccessibles = quarterStatistics.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count;

        var totalStatistics = quarterStatistics.Any(a => a.Type.Contains("total"));
        var iconWidth = totalStatistics ? "400px" : "200px";
        var iconHeight = totalStatistics ? "480px" : "240px";
        var fontSize = totalStatistics ? "16px" : "14px";
        var padding = totalStatistics ? "40px" : "10px";
        var center = totalStatistics ? "text-align:center;padding-right:80px" : "text-align:center;padding-right:20px";


        MapPoint = JsonConvert.SerializeObject(new { Coordinates = new double[] { quarterStatistics.FirstOrDefault().Geography.Coordinate.X, quarterStatistics.FirstOrDefault().Geography.Coordinate.Y } }, serializerSettings);

        HtmlIcon = @$"<div style='width:{iconWidth};height:{iconHeight};position:relative'>
            <img src='css/images/address.png' style='position:absolute;width:100%;opacity:0.9;filter:saturate(0.5)'/>
            <div style='position:absolute;padding:{padding};bottom:0;width:100%'>
            <div style='position:relative;width:100%'>
            <div style='{center}'>
            <p style='font-weight:700;font-size:{fontSize}'>{(CultureInfo.CurrentCulture.Name == "en-US" ? quarterStatistics.FirstOrDefault().QuarterEn.ToUpper() : quarterStatistics.FirstOrDefault().QuarterCs.ToUpper())}</p>
            </div>
            <p style='font-weight:700;font-size:{fontSize}'><img src='css/images/address.png' width=20 height=20 style='opacity:0.8;vertical-align:middle;margin-right:10px' />{victims} {cl["countJews"]}</p>
            <p style='font-weight:700;font-size:{fontSize}'><img src='css/images/incident.png' width=20 height=20 style='opacity:0.8;vertical-align:middle;margin-right:10px' />{incidents} {cl["countIncidents"]}</p>
            <p style='font-weight:700;font-size:{fontSize}'><img src='css/images/interest.png' width=20 height=20 style='opacity:0.8;vertical-align:middle;margin-right:10px' />{interests} {cl["countPointsOfInterest"]}</p>
            <p style='font-weight:700;font-size:{fontSize}'><img src='css/images/interest.png' width=20 height=20 style='opacity:0.8;vertical-align:middle;margin-right:10px' />{inaccessibles} {cl["countInaccessiblePlaces"]}</p>
            {(totalStatistics ? $"<p style='font-weight:700;font-size:{fontSize}'>{cl["zoomMapForMoreInfo"]}" : "")}
            </div>
            </div>
            </div>";

    }
    public string? PlaceType { get; set; }

    public decimal? Citizens { get; set; }

    public decimal? CitizensTotal { get; set; }
    public int? Id { get; set; }

    public string? Guid { get; set; }

    public string? Label { get; set; }

    public string? MapPoint { get; set; }

    public string? MapPolygon { get; set; }
    public string? HtmlIcon { get; set; }

}
