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
using EhriMemoMap.Shared;

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

        if (PlaceType == Shared.PlaceType.Incident.ToString())
            HtmlIcon = "<img src='images/incident-map.png' />";

        else if (PlaceType == Shared.PlaceType.Interest.ToString())
            HtmlIcon = "<img src='images/interest-map.png' />";

        else if (PlaceType == Shared.PlaceType.Memory.ToString())
            HtmlIcon = "<img src='images/memory-map.png' />";

        else if (PlaceType == Shared.PlaceType.Address.ToString())
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

            HtmlIcon = $"<div style='position:relative'><img src='images/addresses/{iconFile}_{Citizens}.png' /></div>";
        }
    }

    public MapObjectForLeafletModel(List<MapStatistic> statistics, IStringLocalizer<CommonResources> cl)
    {
        if (statistics == null || statistics.Count == 0)
            return;

        PlaceType = Shared.PlaceType.Statistics.ToString();
        Id = statistics.FirstOrDefault()?.Id;
        Guid = PlaceType + "_" + statistics.FirstOrDefault()?.Id + "_";
        Clickable = false;
        MapPolygon = statistics.FirstOrDefault(a => !string.IsNullOrEmpty(a.MapPolygon))?.MapPolygon;
        MapPoint = statistics.FirstOrDefault(a => !string.IsNullOrEmpty(a.MapPoint))?.MapPoint;
        CustomTooltipClass = "statistics-tooltip";
        CustomPolygonClass = "statistics-polygon";

        var victims = statistics.FirstOrDefault(a => a.Type.Contains("victims"))?.Count;
        var incidents = statistics.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count;
        var interests = statistics.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count;
        var inaccessibles = statistics.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count;
        var placesOfMemory = statistics.FirstOrDefault(a => a.Type.Contains("places_of_memory"))?.Count;

        Label = @$"<h2 class='rz-mb-2'>{(CultureInfo.CurrentCulture.Name == "en-US" ? statistics.FirstOrDefault().QuarterEn : statistics.FirstOrDefault().QuarterCs)}</h3>

            <table class='statistics-table'>
                    <td class='statistics-table-firstcell'>
                        <span >{cl["countJews"]}</span>
                    </td>
                    <td class='statistics-table-secondcell'>
                        {victims}
                    </td>
                </tr>
                <tr>
                    <td class='statistics-table-firstcell'>
                        <span >{cl["countIncidents"]}</span>
                    </td>
                    <td class='statistics-table-secondcell'>
                        {incidents}
                    </td>
                </tr>
                <tr>
                    <td class='statistics-table-firstcell'>
                        <span >{cl["countPointsOfInterest"]}</span>
                    </td>
                    <td class='statistics-table-secondcell'>
                        {interests}
                    </td>
                </tr>
                <tr>
                    <td class='statistics-table-firstcell'>
                        <span>{cl["countInaccessiblePlaces"]}</span>
                    </td>
                    <td class='statistics-table-secondcell'>
                        {inaccessibles}
                    </td>
                </tr>
                <tr>
                    <td class='statistics-table-firstcell'>
                        <span>{cl["countPlacesOfMemory"]}</span>
                    </td>
                    <td class='statistics-table-secondcell'>
                        {placesOfMemory}
                    </td>
                </tr>
                <tr>
                    <td colspan=2>
                        {cl["forDetailsZoom"]}
                    </td>
                </tr>
            </table>";

    }
    public bool Clickable { get; set; }
    public string? PlaceType { get; set; }

    public decimal? Citizens { get; set; }

    public decimal? CitizensTotal { get; set; }
    public long? Id { get; set; }

    public string? Guid { get; set; }

    public string? Label { get; set; }
    public string? CustomTooltipClass { get; set; }
    public string? CustomPolygonClass { get; set; }

    public string? MapPoint { get; set; }

    public string? MapPolygon { get; set; }
    public string? HtmlIcon { get; set; }

}
