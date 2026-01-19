using EhriMemoMap.Models;
using EhriMemoMap.Shared;
using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Text;

namespace EhriMemoMap.Client.Services
{
    /// <summary>
    /// Logika nad mapou
    /// </summary>
    public partial class MapService
    {
        
        public async Task RefreshObjectsOnMap(bool withPolygons)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var objects = await GetMapObjects(withPolygons, true);

            var statistics = await GetDistrictStatistics();
            objects.AddRange(statistics);

            var heatmap = await GetHeatMap();
            objects.AddRange(heatmap);

            var result = JsonConvert.SerializeObject(objects, serializerSettings);
            await _js.InvokeVoidAsync("mapAPI.refreshObjectsOnMap", result);
        }

        public async Task<List<MapObjectForLeafletModel>> GetMapObjects(bool withPolygons, bool aggregate, Coordinate[]? customCoordinates = null)
        {
            if (Map == null)
                return [];

            var parameters = new MapObjectParameters();

            // vyfiltruju objekty podle toho, na jakem bode casove ose lezi
            if (Map.Timeline != null && Map.Timeline.Any(a => a.Selected))
                parameters.SelectedTimeLinePoint = Map.Timeline.FirstOrDefault(a => a.Selected);

            // vyber objektu podle toho, do jake nalezi vrstvy
            var selectedLayerNames = new List<string?>();
            foreach (var layer in GetNotBaseLayers(true).Where(a => a.Type != LayerType.Heatmap && a.PlaceType != null && (withPolygons || a.Type != LayerType.Polygons)))
            {
                // krome nazvu vrstvy zkoumam i to, jestli se pri danem zoomu mapy ma vrstva vubec zobrazovat
                if (!ShowLayersForce && ((layer.MinZoom != null && MapZoom < layer.MinZoom) || (layer.MaxZoom != null && MapZoom > layer.MaxZoom)))
                    continue;
                selectedLayerNames.Add(layer.PlaceType?.ToString());
            }
            parameters.SelectedLayerNames = selectedLayerNames;
            parameters.CustomCoordinates = customCoordinates?.Select(a => new PointModel { X = a.X, Y = a.Y }).ToArray();
            parameters.MapSouthWestPoint = new PointModel { X = MapSouthWestPoint.X, Y = MapSouthWestPoint.Y };
            parameters.MapNorthEastPoint = new PointModel { X = MapNorthEastPoint.X, Y = MapNorthEastPoint.Y };
            parameters.City = Map.InitialVariables?.City;

            var objects = await GetResultFromApiPost<List<MapObject>>("getmapobjects", parameters);

            if (aggregate)
                objects = AggregateSameAddresses(objects);

            return objects.Select(a => new MapObjectForLeafletModel(a, false, Map.Layers)).ToList();
        }

        /// <summary>
        /// Slouci objekty se stejnou adresou do jednoho objektu 
        /// (používá se v případě, že některé adresy mají stejné souřadnice, 
        /// protože se tyto souřadnice zřejmě nepodařilo dohledat)
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public List<MapObject> AggregateSameAddresses(List<MapObject>? objects)
        {
            var result = new List<MapObject>();

            if (objects == null)
                return result;

            var objectGroups = objects.GroupBy(a => a.PlaceType);

            foreach (var objectGroup in objectGroups)
            {
                var grouppedAddresses = objectGroup.GroupBy(a => new { a.GeographyMapPoint?.Coordinate.X, a.GeographyMapPoint?.Coordinate.Y });
                foreach (var grouppedAddress in grouppedAddresses)
                {
                    if (grouppedAddress.FirstOrDefault() == null)
                        continue;

                    if (grouppedAddress.Count() == 1)
                    {
                        result.Add(grouppedAddress.First());
                        continue;
                    }

                    var grouppedObject = grouppedAddress.First();
                    grouppedObject.Citizens = grouppedAddress.Sum(a => a.Citizens);
                    grouppedObject.CitizensTotal = grouppedAddress.Sum(a => a.CitizensTotal);
                    grouppedObject.LabelCs = grouppedAddress.Select(a => a.LabelCs).Distinct().Aggregate((x, y) => x + " <br/> " + y);
                    grouppedObject.LabelEn = grouppedAddress.Select(a => a.LabelEn).Distinct().Aggregate((x, y) => x + " <br/> " + y);

                    result.Add(grouppedObject);

                }
            }

            return result;
        }

        public async Task<List<MapObjectForLeafletModel>> GetDistrictStatistics()
        {
            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            var layer = GetNotBaseLayers().FirstOrDefault(a => a.Selected && a.PlaceType == PlaceType.Statistics);

            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            if (layer == null)
                return [];

            if (!(MapZoom >= layer.MinZoom && MapZoom <= layer.MaxZoom) && !(MapZoom < layer.MinZoom && MapZoom >= 0))
                return [];

            var statistics = new List<MapStatistic>();
            var parameters = new DistrictStatisticsParameters { Total = true, TimeLinePoint = GetTimelinePoint() };

            // pokud je zoom vetsi nez maximalni zoom vrstvy, zobrazim statistiky pro jednotlive casti Prahy
            if (MapZoom >= layer.MinZoom && MapZoom <= layer.MaxZoom)
                parameters.Total = false;

            parameters.City = Map.InitialVariables?.City;

            statistics = await GetResultFromApiGet<List<MapStatistic>>("getdistrictstatistics", $"city={parameters.City}&total={parameters.Total}{(parameters.TimeLinePoint != null ? "&timeLinePoint=" + parameters.TimeLinePoint?.ToString("yyyy-MM-dd") : "")}");

            return statistics.GroupBy(a => a.QuarterCs).Select(a => new MapObjectForLeafletModel(a.ToList(), _cl)).ToList();
        }

        public async Task<List<MapObjectForLeafletModel>> GetHeatMap(Coordinate[]? customCoordinates = null)
        {
            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            var layer = GetNotBaseLayers().FirstOrDefault(a => a.Selected && a.Type == LayerType.Heatmap);

            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            if (layer == null)
                return [];

            var parameters = new MapObjectParameters
            {
                SelectedLayerNames = [layer.PlaceType?.ToString()],
                SelectedTimeLinePoint = new TimelinePointModel { From = null, To = null },
                CustomCoordinates = customCoordinates?.Select(a => new PointModel { X = a.X, Y = a.Y }).ToArray(),
                MapSouthWestPoint = new PointModel { X = MapSouthWestPoint.X, Y = MapSouthWestPoint.Y },
                MapNorthEastPoint = new PointModel { X = MapNorthEastPoint.X, Y = MapNorthEastPoint.Y },
                City = Map.InitialVariables?.City
            };
            var heatmap = await GetResultFromApiPost<List<MapObject>>("getheatmap", parameters);

            return heatmap.Select(a => new MapObjectForLeafletModel(a, true)).ToList();
        }

        public async Task<WelcomeDialogStatistics> GetWelcomeDialogStatistics()
        {
            var statistics = await GetResultFromApiGet<WelcomeDialogStatistics>("getwelcomedialogstatistics", "city=" + Map.InitialVariables?.City);
            return statistics;
        }

        public async Task<PlacesResult> GetPlaces(PlacesParameters parameters)
        {
            var result = await GetResultFromApiPost<PlacesResult>("getplaces", parameters);
            return result;
        }

        public async Task<List<SolrPlace>> GetSolrPlaces(SolrQueryParameters parameters)
        {
            var result = await GetResultFromApiPost<List<SolrPlace>>("getsolrplaces", parameters);
            return result;
        }

        public async Task<T> GetResultFromApiPost<T>(string apiMethod, object? parameters)
        {
            var json = JsonConvert.SerializeObject(parameters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiResult = await _client.PostAsync(_apiUrl + apiMethod, content);
            var jsonString = await apiResult.Content.ReadAsStringAsync();

            using var stringReader = new StringReader(jsonString);
            using var jsonReader = new JsonTextReader(stringReader);

            var serializer = GeoJsonSerializer.Create();
            var result = serializer.Deserialize<T>(jsonReader);
            return result;

        }

        public async Task<T> GetResultFromApiGet<T>(string apiMethod, string parameters)
        {
            var jsonString = await _client.GetStringAsync(_apiUrl + apiMethod + "?" + parameters);

            using var stringReader = new StringReader(jsonString);
            using var jsonReader = new JsonTextReader(stringReader);

            var serializer = GeoJsonSerializer.Create();
            var result = serializer.Deserialize<T>(jsonReader);
            return result;

        }

        public async Task<VictimLongInfoModel?> GetVictimLongInfo(long? id)
        {
            if (id == null)
                return null;
            var result = await GetResultFromApiGet<VictimLongInfoModel>("getvictimlonginfo", "city=" + Map.InitialVariables?.City + "&id=" + id);
            return result;
        }

        /////////////////////
        /// NARRATIVE MAP ///
        /////////////////////



        public async Task ShowPlacesOnMap(IEnumerable<Place>? places, bool defaultColor = true)
        {
            if (places == null || !places.Any())
                return;

            var transformedPlaces = places.
                Select(b => new MapObjectForLeafletModel(b, defaultColor)).ToList();

            if (transformedPlaces.Any(a => a.PlaceType == "trajectory point"))
            {
                transformedPlaces.LastOrDefault(a => a.PlaceType == "trajectory point").HtmlIcon = "<img src='css/images/narrative-icon.png' />";
                transformedPlaces.LastOrDefault(a => a.PlaceType == "trajectory point").IconAnchor = [22,55];
                transformedPlaces.LastOrDefault(a => a.PlaceType == "trajectory point").Clickable = true;

            }

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var jsonPlaces = JsonConvert.SerializeObject(transformedPlaces, serializerSettings);
            await _js.InvokeVoidAsync("mapAPI.addObjectsFromJsonString", jsonPlaces, LayerType.Narration.ToString());
        }


        public async Task ShowVictim(long? id)
        {
            VictimLongInfo = await GetVictimLongInfo(id);
            await SetDialog(DialogTypeEnum.Victim, new DialogParameters { Id = VictimLongInfo?.Id, Places = DialogParameters.Places });
            NotifyStateChanged();

        }

    }
}
