using EhriMemoMap.Data;
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
using System.Net.Http.Json;
using System.Numerics;

namespace EhriMemoMap.Services
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
            foreach (var layer in _mapState.GetNotBaseLayers(true).Where(a => a.PlaceType != null && (withPolygons || a.Type != LayerType.Polygons)))
            {
                // krome nazvu vrstvy zkoumam i to, jestli se pri danem zoomu mapy ma vrstva vubec zobrazovat
                if (!_mapState.ShowLayersForce && ((layer.MinZoom != null && _mapState.MapZoom < layer.MinZoom) || (layer.MaxZoom != null && _mapState.MapZoom > layer.MaxZoom)))
                    continue;
                selectedLayerNames.Add(layer.PlaceType?.ToString());
            }
            parameters.SelectedLayerNames = selectedLayerNames;
            parameters.CustomCoordinates = customCoordinates?.Select(a=> new PointModel { X = a.X, Y = a.Y }).ToArray();
            parameters.MapSouthWestPoint = new PointModel { X = _mapState.MapSouthWestPoint.X, Y = _mapState.MapSouthWestPoint.Y };
            parameters.MapNorthEastPoint = new PointModel { X = _mapState.MapNorthEastPoint.X, Y = _mapState.MapNorthEastPoint.Y };

            var objects = await GetResultFromApiPost<List<MapObject>>("getmapobjects", parameters);

            if (aggregate)
                objects = AggregateSameAddresses(objects);

            return objects.Select(a => new MapObjectForLeafletModel(a)).ToList();
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


            statistics = await GetResultFromApiGet<List<MapStatistic>>("getdistrictstatistics", $"total={parameters.Total}{(parameters.TimeLinePoint != null ? "&timeLinePoint=" + parameters.TimeLinePoint?.ToString("yyyy-MM-dd") : "")}");
            
            return statistics.GroupBy(a => a.QuarterCs).Select(a => new MapObjectForLeafletModel(a.ToList(), _cl)).ToList();
        }

        public async Task<WelcomeDialogStatistics> GetWelcomeDialogStatistics()
        {
            var statistics = await GetResultFromApiPost<WelcomeDialogStatistics>("getwelcomedialogstatistics", null);
            return statistics;
        }

        public async Task<PlacesResult> GetPlaces(PlacesParameters parameters)
        {
            var result = await GetResultFromApiPost<PlacesResult>("getplaces", parameters);
            return result;
        }

        public async Task<List<Place>> GetSolrPlaces(SolrQueryParameters parameters)
        {
            var result = await GetResultFromApiPost<List<Place>>("getsolrplaces", parameters);
            return result;
        }

        private async Task<T> GetResultFromApiPost<T>(string apiMethod, object? parameters)
        {
            var apiResult = await _client.PostAsJsonAsync(_apiUrl + apiMethod, parameters);
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
    }
}
