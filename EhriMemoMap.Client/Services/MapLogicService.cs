using EhriMemoMap.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Radzen;
using Microsoft.JSInterop;
using Microsoft.Extensions.Localization;
using EhriMemoMap.Resources;
using NetTopologySuite.IO;
using EhriMemoMap.Shared;
using System.Text;

namespace EhriMemoMap.Client.Services
{
    /// <summary>
    /// Logika nad mapou
    /// </summary>
    public class MapLogicService
    {
        private readonly MapStateService _mapState;
        private readonly IJSRuntime _js;
        private readonly IStringLocalizer<CommonResources> _cl;
        private readonly HttpClient _client;
        private readonly string _apiUrl;

        public MapLogicService(MapStateService mapState, IJSRuntime js, IStringLocalizer<CommonResources> cl, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _mapState = mapState;
            _js = js;
            _cl = cl;
            _client = clientFactory.CreateClient();
            _apiUrl = configuration.GetSection("App")["ApiURL"] ?? "";


        }

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
            if (_mapState == null || _mapState.Map == null)
                return [];

            var parameters = new MapObjectParameters();

            // vyfiltruju objekty podle toho, na jakem bode casove ose lezi
            if (_mapState.Map.Timeline != null && _mapState.Map.Timeline.Any(a => a.Selected))
                parameters.SelectedTimeLinePoint = _mapState.Map.Timeline.FirstOrDefault(a => a.Selected);

            // vyber objektu podle toho, do jake nalezi vrstvy
            var selectedLayerNames = new List<string?>();
            foreach (var layer in _mapState.GetNotBaseLayers(true).Where(a => a.Type != LayerType.Heatmap && a.PlaceType != null && (withPolygons || a.Type != LayerType.Polygons)))
            {
                // krome nazvu vrstvy zkoumam i to, jestli se pri danem zoomu mapy ma vrstva vubec zobrazovat
                if (!_mapState.ShowLayersForce && ((layer.MinZoom != null && _mapState.MapZoom < layer.MinZoom) || (layer.MaxZoom != null && _mapState.MapZoom > layer.MaxZoom)))
                    continue;
                selectedLayerNames.Add(layer.PlaceType?.ToString());
            }
            parameters.SelectedLayerNames = selectedLayerNames;
            parameters.CustomCoordinates = customCoordinates?.Select(a => new PointModel { X = a.X, Y = a.Y }).ToArray();
            parameters.MapSouthWestPoint = new PointModel { X = _mapState.MapSouthWestPoint.X, Y = _mapState.MapSouthWestPoint.Y };
            parameters.MapNorthEastPoint = new PointModel { X = _mapState.MapNorthEastPoint.X, Y = _mapState.MapNorthEastPoint.Y };
            parameters.City = _mapState.Map.InitialVariables?.City;

            var objects = await GetResultFromApiPost<List<MapObject>>("getmapobjects", parameters);

            if (aggregate)
                objects = AggregateSameAddresses(objects);

            return objects.Select(a => new MapObjectForLeafletModel(a, false, _mapState.Map.Layers)).ToList();
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
                    grouppedObject.LabelCs = grouppedAddress.Select(a => a.LabelCs).Aggregate((x, y) => x + " <br/> " + y);
                    grouppedObject.LabelEn = grouppedAddress.Select(a => a.LabelEn).Aggregate((x, y) => x + " <br/> " + y);

                    result.Add(grouppedObject);

                }
            }

            return result;
        }

        public async Task<List<MapObjectForLeafletModel>> GetDistrictStatistics()
        {
            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            var layer = _mapState.GetNotBaseLayers().FirstOrDefault(a => a.Selected && a.PlaceType == PlaceType.Statistics);

            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            if (layer == null)
                return [];

            if (!(_mapState.MapZoom >= layer.MinZoom && _mapState.MapZoom <= layer.MaxZoom) && !(_mapState.MapZoom < layer.MinZoom && _mapState.MapZoom >= 0))
                return [];

            var statistics = new List<MapStatistic>();
            var parameters = new DistrictStatisticsParameters { Total = true, TimeLinePoint = _mapState.GetTimelinePoint() };

            // pokud je zoom vetsi nez maximalni zoom vrstvy, zobrazim statistiky pro jednotlive casti Prahy
            if (_mapState.MapZoom >= layer.MinZoom && _mapState.MapZoom <= layer.MaxZoom)
                parameters.Total = false;

            parameters.City = _mapState.Map.InitialVariables?.City;

            statistics = await GetResultFromApiGet<List<MapStatistic>>("getdistrictstatistics", $"city={parameters.City}&total={parameters.Total}{(parameters.TimeLinePoint != null ? "&timeLinePoint=" + parameters.TimeLinePoint?.ToString("yyyy-MM-dd") : "")}");

            return statistics.GroupBy(a => a.QuarterCs).Select(a => new MapObjectForLeafletModel(a.ToList(), _cl)).ToList();
        }

        public async Task<List<MapObjectForLeafletModel>> GetHeatMap(Coordinate[]? customCoordinates = null)
        {
            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            var layer = _mapState.GetNotBaseLayers().FirstOrDefault(a => a.Selected && a.Type == LayerType.Heatmap);

            // pokud neni zadna vrstva s statistikami vybrana, vratim prazdny seznam
            if (layer == null)
                return [];

            var parameters = new MapObjectParameters
            {
                SelectedLayerNames = [layer.PlaceType?.ToString()],
                SelectedTimeLinePoint = new TimelinePointModel { From = null, To = null },
                CustomCoordinates = customCoordinates?.Select(a => new PointModel { X = a.X, Y = a.Y }).ToArray(),
                MapSouthWestPoint = new PointModel { X = _mapState.MapSouthWestPoint.X, Y = _mapState.MapSouthWestPoint.Y },
                MapNorthEastPoint = new PointModel { X = _mapState.MapNorthEastPoint.X, Y = _mapState.MapNorthEastPoint.Y },
                City = _mapState.Map.InitialVariables?.City
            };
            var heatmap = await GetResultFromApiPost<List<EhriMemoMap.Shared.MapObject>>("getheatmap", parameters);

            return heatmap.Select(a => new MapObjectForLeafletModel(a, true)).ToList();
        }

        public async Task<WelcomeDialogStatistics> GetWelcomeDialogStatistics()
        {
            var statistics = await GetResultFromApiGet<WelcomeDialogStatistics>("getwelcomedialogstatistics", "city=" + _mapState.Map.InitialVariables?.City);
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

        private async Task<T> GetResultFromApiPost<T>(string apiMethod, object? parameters)
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

        private async Task<T> GetResultFromApiGet<T>(string apiMethod, string parameters)
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
            var result = await GetResultFromApiGet<VictimLongInfoModel>("getvictimlonginfo", "city=" + _mapState.Map.InitialVariables?.City + "&id=" + id);
            return result;
        }

        /////////////////////
        /// NARRATIVE MAP ///
        /////////////////////

        public async Task GetAllNarrativeMaps()
        {
            if (_mapState.AllNarrativeMaps != null && _mapState.AllNarrativeMaps.Count > 0)
                return;

            var result = await GetResultFromApiGet<List<NarrativeMap>>("getallnarrativemaps", "city=" + _mapState.Map.InitialVariables?.City);
            _mapState.AllNarrativeMaps = result;
        }

        public async Task GetNarrativeMap(long? id, string? city)
        {
            if (id == null)
                return;

            var result = await GetResultFromApiGet<NarrativeMap>("getnarrativemap", "id=" + id + "&city=" + city);
            _mapState.NarrativeMap = result;
        }

        public async Task ShowNarrativeMapPlaces()
        {
            if (_mapState.NarrativeMap == null)
                return;
            await ShowPlacesOnMap(_mapState.NarrativeMap?.Stops?.SelectMany(a => a.Places!).Where(a => a.Type == "main point"));
            _mapState.SetMapType(MapTypeEnum.StoryMapWhole, false);
        }

        public async Task ShowPlacesOnMap(IEnumerable<Place>? places)
        {
            if (places == null || !places.Any())
                return;

            var transformedPlaces = places.
                Select(b => new MapObjectForLeafletModel(b)).ToList();

            if (transformedPlaces.Any(a => a.PlaceType == "trajectory point"))
            {
                transformedPlaces.LastOrDefault(a => a.PlaceType == "trajectory point").HtmlIcon = "<img src='css/images/narrative-icon.png' />";
                transformedPlaces.LastOrDefault(a => a.PlaceType == "trajectory point").Clickable = true;

            }

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var jsonPlaces = JsonConvert.SerializeObject(transformedPlaces, serializerSettings);
            await _js.InvokeVoidAsync("mapAPI.addObjectsFromJsonString", jsonPlaces, LayerType.Narration.ToString());
        }

        public async Task ShowStopPlacesOnMap(long stopId)
        {
            _mapState.SetMapType(MapTypeEnum.StoryMapOneStop, false);
            var stop = _mapState.NarrativeMap?.Stops?.FirstOrDefault(a => a.Id == stopId);
            if (stop == null)
                return;
            await ShowPlacesOnMap(stop.Places);
        }
    }
}
