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

        Clickable = true;
        PlaceType = mapObject.PlaceType;
        Citizens = mapObject.Citizens;
        CitizensTotal = mapObject.CitizensTotal;
        Id = mapObject.Id;
        Guid = mapObject.PlaceType + "_" + mapObject.Id + "_" + mapObject.DateFrom?.ToString("yyyy-mm-dd");
        Label = CultureInfo.CurrentCulture.Name == "en-US" ? mapObject.LabelEn : mapObject.LabelCs;
        MapPoint = mapObject.MapPoint;
        MapPolygon = mapObject.MapPolygon;

        if (PlaceType == Models.PlaceType.Incident.ToString())
            HtmlIcon = "<img src='images/incident-map.png' height=23 width=23 style='opacity:1'/>";

        else if (PlaceType == Models.PlaceType.Interest.ToString())
            HtmlIcon = "<img src='images/interest-map.png' height=23 width=23 style='opacity:1'/>";

        else if (PlaceType == Models.PlaceType.Address.ToString())
        {
            var percents = (decimal)Citizens / CitizensTotal;
            string iconFile = percents switch
            { 
                1 => "victims_100",
                < 1 and >= 0.5M => "victims_051",
                < 0.5M and > 0 => "victims_001",
                0 => "victims_000",
                _ => "victims_100"
            };

            HtmlIcon = $"<div style='position:relative'><img src='images/addresses/{iconFile}_{Citizens}.png' height=23 width=23 style='opacity:1;'/></div>";
        }
    }

    public MapObjectForLeafletModel(List<PragueQuartersStat> quarterStatistics, IStringLocalizer<CommonResources> cl, bool isMobileBrowser)
    {
        if (quarterStatistics == null || quarterStatistics.Count == 0)
            return;

        PlaceType = Models.PlaceType.Statistics.ToString();
        Guid = PlaceType + "_" + Id + "_";
        Clickable = false;

        var serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var victims = quarterStatistics.FirstOrDefault(a => a.Type.Contains("victims"))?.Count;
        var incidents = quarterStatistics.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count;
        var interests = quarterStatistics.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count;
        var inaccessibles = quarterStatistics.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count;

        MapPoint = JsonConvert.SerializeObject(new { Coordinates = new double[] { quarterStatistics.FirstOrDefault().Geography.Coordinate.X, quarterStatistics.FirstOrDefault().Geography.Coordinate.Y } }, serializerSettings);

        HtmlIcon = @$"
            <table style='width:{(isMobileBrowser ? "350px" : "500px")}'>
                <tr >
                    <td colspan='2'>
                        <span class='statistics-title-text' style='margin-bottom:20px'>{(CultureInfo.CurrentCulture.Name == "en-US" ? quarterStatistics.FirstOrDefault().QuarterEn.ToUpper() : quarterStatistics.FirstOrDefault().QuarterCs.ToUpper())}</span>
                    </td>       
                <tr>
                    <td style='width:10%;vertical-align:top;'>
                        <img src='css/images/address.png' width=50 height=50 style='opacity:0.8;vertical-align:middle;margin-right:10px' />
                    </td>
                    <td style='width:90%;vertical-align:top;'>
                        <span class='statistics-title-text'>{victims} {cl["countJews"]}</span>
                    </td>
                </tr>
                <tr>
                    <td style='width:10%;vertical-align:top;'>
                        <img src='css/images/incident.png' width=50 height=50 style='opacity:0.8;vertical-align:middle;margin-right:10px' />
                    </td>
                    <td style='width:90%;vertical-align:top;'>
                        <span class='statistics-title-text'>{incidents} {cl["countIncidents"]}</span>
                    </td>
                </tr>
                <tr>
                    <td style='width:10%;vertical-align:top;'>
                        <img src='css/images/interest.png' width=50 height=50 style='opacity:0.8;vertical-align:middle;margin-right:10px' />
                    </td>
                    <td style='width:90%;vertical-align:top;'>
                        <span class='statistics-title-text'>{interests} {cl["countPointsOfInterest"]}</span>
                    </td>
                </tr>
                <tr>
                    <td style='width:10%;vertical-align:top;'>
                        <img src='css/images/polygon.png' width=50 height=50 style='opacity:0.8;vertical-align:middle;margin-right:10px' />
                    </td>
                    <td style='width:90%;vertical-align:top;'>
                        <span class='statistics-title-text'>{inaccessibles} {cl["countInaccessiblePlaces"]}</span>
                    </td>
                </tr>
                <tr>
                    <td style='width:10%;vertical-align:top;'>
                    </td>
                    <td style='width:90%;vertical-align:top;'>
                        <span class='statistics-title-text' style='font-size:20px'>{cl["zoomMapForMoreInfo"]}</span>
                    </td>
                </tr>
            </table>";

    }
    public bool Clickable { get; set; }
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
