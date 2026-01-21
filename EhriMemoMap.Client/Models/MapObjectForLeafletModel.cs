using System.Globalization;
using EhriMemoMap.Resources;
using Microsoft.Extensions.Localization;
using EhriMemoMap.Shared;

namespace EhriMemoMap.Models;

public partial class MapObjectForLeafletModel
{
    public MapObjectForLeafletModel() { }

    public MapObjectForLeafletModel(SolrPlace? place)
    {
        if (place == null)
            return;

        Id = place.Id;
        Label = place.PlaceCs;
        MapPoint = place.MapLocation;
        Clickable = true;
        IconAnchor = [13,45];

        HtmlIcon = Id.Contains("victim_last_residence") 
            ? "<img src='css/images/marker-icon-red.png' />" 
            : "<img src='css/images/marker-icon.png' />";
    }

    public MapObjectForLeafletModel(Place place, bool defaultColor = true)
    {
        Id = place.Id.ToString();
        Label = place.LabelCs;
        MapPoint = place.MapPoint;
        PlaceType = place.Type;
        StopId = place.StopId;
        NarrativeMapId = place.NarrativeMapId;

        switch (place.Type)
        {
            case "main point":
                var iconNumber = place.NarrativeMapId % 7;
                HtmlIcon = $"<img src='css/images/narrative-icon{(defaultColor ? "" : iconNumber)}.png' />";
                Clickable = true;
                IconAnchor = [22, 55];
                break;
            case "trajectory point":
                HtmlIcon = "<img src='css/images/trajectory-icon.png' />";
                IconAnchor = [10, 10];
                break;
            default:
                HtmlIcon = "<img src='css/images/marker-icon-wine-red.png' />";
                IconAnchor = [22, 41];
                break;
        }
    }

    public MapObjectForLeafletModel(Shared.MapObject mapObject, bool heatmap, List<LayerModel>? layers = null)
    {
        Clickable = !heatmap;
        Citizens = mapObject.Citizens;
        MapPoint = mapObject.MapPoint;
        PlaceType = mapObject.PlaceType;
        Heatmap = heatmap;
        LayerName = mapObject.LayerName;
        PriorityOnMap = layers?.FirstOrDefault(a => !string.IsNullOrEmpty(a.Name) && a.Name?.ToLower() == mapObject.PlaceType?.ToLower())?.PriorityOnMap;

        if (heatmap)
            return;

        CitizensTotal = mapObject.CitizensTotal;
        Id = mapObject.Id.ToString();
        Guid = mapObject.PlaceType + "_" + mapObject.Id + "_" + mapObject.DateFrom?.ToString("yyyy-mm-dd");
        Label = CultureInfo.CurrentCulture.Name == "en-US" ? mapObject.LabelEn : mapObject.LabelCs;
        MapPolygon = mapObject.MapPolygon;

        if (PlaceType == Shared.PlaceType.Incident.ToString())
        {
            HtmlIcon = "<img src='images/incident-map.png' />";
            IconAnchor = [12, 12];
        }
        else if (PlaceType == Shared.PlaceType.Interest.ToString())
        {
            HtmlIcon = "<img src='images/interest-map.png' />";
            IconAnchor = [12, 12];
        }
        else if (PlaceType == Shared.PlaceType.Memory.ToString())
        {
            HtmlIcon = "<img src='images/memory-map.png' />";
            IconAnchor = [20, 20];
        }
        else if (PlaceType == Shared.PlaceType.Memorial.ToString())
        {
            HtmlIcon = "<img src='images/memorial-map.png' />";
            IconAnchor = [12, 12];
        }
        else if (PlaceType == Shared.PlaceType.Address.ToString() || PlaceType == Shared.PlaceType.AddressLastResidence.ToString())
        {
            if (Citizens == null)
            {
                HtmlIcon = $"<div style='{(PlaceType == Shared.PlaceType.AddressLastResidence.ToString() ? "filter:invert(1);" : "")}position:relative'><img src='images/addresses/victims_100.png' /></div>";
            }
            else
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

                HtmlIcon = $"<div style='{(PlaceType == Shared.PlaceType.AddressLastResidence.ToString() ? "filter:invert(1);" : "")}position:relative'><img src='images/addresses/{iconFile}_{Citizens}.png' /></div>";
            }
            IconAnchor = [12, 12];
        }
    }

    public MapObjectForLeafletModel(List<Shared.MapStatistic> statistics, IStringLocalizer<CommonResources> cl)
    {
        if (statistics == null || statistics.Count == 0)
            return;

        PlaceType = Shared.PlaceType.Statistics.ToString();
        Id = statistics.FirstOrDefault()?.Id?.ToString();
        Guid = PlaceType + "_" + statistics.FirstOrDefault()?.Id + "_";
        Clickable = false;
        MapPolygon = statistics.FirstOrDefault(a => !string.IsNullOrEmpty(a.MapPolygon))?.MapPolygon;
        MapPoint = statistics.FirstOrDefault(a => !string.IsNullOrEmpty(a.MapPoint))?.MapPoint;
        CustomTooltipClass = "statistics-tooltip";
        CustomPolygonClass = "statistics-polygon";

        var victims = statistics.FirstOrDefault(a => a.Type.Contains("victims"))?.Count ?? 0;
        var incidents = statistics.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count ?? 0;
        var interests = statistics.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count ?? 0;
        var inaccessibles = statistics.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count ?? 0;
        var placesOfMemory = statistics.FirstOrDefault(a => a.Type.Contains("places_of_memory"))?.Count ?? 0;

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
    public string? LayerName { get; set; }

    public decimal? Citizens { get; set; }

    public decimal? CitizensTotal { get; set; }
    public string? Id { get; set; }

    public string? Guid { get; set; }

    public string? Label { get; set; }
    public string? CustomTooltipClass { get; set; }
    public string? CustomPolygonClass { get; set; }

    public string? MapPoint { get; set; }

    public string? MapPolygon { get; set; }
    public string? HtmlIcon { get; set; }
    public string? IconUrl { get; set; }
    public int[] IconAnchor { get; set; } 
    public int[] IconSize { get; set; }

    public bool? Heatmap { get; set; }
    public long? StopId { get; set; }
    public long? NarrativeMapId { get; set; }
    public int? PriorityOnMap { get; set; }

}
