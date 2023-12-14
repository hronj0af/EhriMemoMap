﻿using EhriMemoMap.Data;
using EhriMemoMap.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Radzen;
using Microsoft.JSInterop;
using Microsoft.Extensions.Localization;
using EhriMemoMap.Resources;

namespace EhriMemoMap.Services
{
    /// <summary>
    /// Logika nad mapou
    /// </summary>
    public class MapLogicService
    {
        private readonly MapStateService _mapState;
        private readonly MemogisContext _context;
        private readonly IJSRuntime _js;
        private readonly IStringLocalizer<CommonResources> _cl;


        public MapLogicService(MapStateService mapState, MemogisContext context, IJSRuntime js, IStringLocalizer<CommonResources> cl)
        {
            _mapState = mapState;
            _context = context;
            _js = js;
            _cl = cl;
        }

        public async Task RefreshObjectsOnMap(bool withPolygons)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var objects = GetMapObjects(withPolygons);
            var statistics = GetDistrictStatistics();
            objects.AddRange(statistics);

            var result = JsonConvert.SerializeObject(objects, serializerSettings);
            await _js.InvokeVoidAsync("mapAPI.refreshObjectsOnMap", result);
        }

        public List<MapObjectForLeafletModel> GetMapObjects(bool withPolygons, Coordinate[]? customCoordinates = null)
        {
            if (_mapState == null || _mapState.Map == null)
                return new List<MapObjectForLeafletModel>();

            // nejdriv si pripravim mapove objekty pro dalsi dotazy
            var query = _context.MapObjects.AsQueryable();

            // vyfiltruju objekty podle toho, na jakem bode casove ose lezi
            if (_mapState.Map.Timeline != null && _mapState.Map.Timeline.Any(a => a.Selected))
            {
                var timelimePoint = _mapState.Map.Timeline.FirstOrDefault(a => a.Selected);

                // bude vyberu vsechny objekty, ktere nemaji prirazeno zadne datum
                if (timelimePoint != null && timelimePoint.From == null && timelimePoint.To == null)
                    query = query.Where(a => a.DateFrom == null && a.DateTo == null);

                // anebo vyberu objekty, ktere lezi v casovem intervalu daneho bodu casove osy
                else if (timelimePoint != null)
                    query = query.Where(a => (a.DateFrom >= timelimePoint.From && a.DateTo <= timelimePoint.To) || (a.DateFrom == null && a.DateTo == null));
            }

            // vyber objektu podle toho, do jake nalezi vrstvy
            var selectedLayerNames = new List<string?>();
            foreach (var layer in _mapState.GetNotBaseLayers(true).Where(a => a.PlaceType != null && (withPolygons || a.Type != LayerType.Polygons)))
            {
                // krome nazvu vrstvy zkoumam i to, jestli se pri danem zoomu mapy ma vrstva vubec zobrazovat
                if ((layer.MinZoom != null && _mapState.MapZoom < layer.MinZoom) || (layer.MaxZoom != null && _mapState.MapZoom > layer.MaxZoom))
                    continue;
                selectedLayerNames.Add(layer.PlaceType?.ToString());
            }

            query = query.Where(a => a.PlaceType != null && selectedLayerNames.Contains(a.PlaceType));

            // a nakonec vyberu ty objekty, ktere jsou viditelne na zobrazenem vyrezu na mape
            if (customCoordinates != null && customCoordinates.Length == 2)
            {
                var bbox = GetBBox(customCoordinates[0], customCoordinates[1]);
                query = query.Where(a => (a.GeographyMapPoint != null && bbox.Intersects(a.GeographyMapPoint)) || (a.GeographyMapPolygon != null && bbox.Intersects(a.GeographyMapPolygon)));
            }
            else if (_mapState.MapSouthWestPoint != null && _mapState.MapNorthEastPoint != null)
            {
                var bbox = GetBBox(_mapState.MapSouthWestPoint, _mapState.MapNorthEastPoint);
                query = query.Where(a => !string.IsNullOrEmpty(a.MapPolygon) || (a.GeographyMapPoint != null && a.GeographyMapPoint.Intersects(bbox)));
            }

            var result = query.Select(a => new MapObjectForLeafletModel(a)).ToList();

            return result;
        }

        public List<MapObjectForLeafletModel> GetDistrictStatistics()
        {
            var layer = _mapState.GetNotBaseLayers().FirstOrDefault(a => a.Selected && a.PlaceType == PlaceType.Statistics);
            
            if (layer == null) 
                return new List<MapObjectForLeafletModel>();

            if (_mapState.MapZoom < layer.MinZoom && _mapState.MapZoom >= layer.MinZoom - 2)
                return _context.MapStatistics.Where(a => a.Type.Contains("total")).GroupBy(a => a.QuarterCs).Select(a => new MapObjectForLeafletModel(a.ToList(), _cl, _mapState.IsMobileBrowser)).ToList();

            if (_mapState.MapZoom >= layer.MinZoom && _mapState.MapZoom <= layer.MaxZoom)
                return _context.MapStatistics.Where(a => !a.Type.Contains("total")).GroupBy(a => a.QuarterCs).Select(a => new MapObjectForLeafletModel(a.ToList(), _cl, _mapState.IsMobileBrowser)).ToList();

            return new List<MapObjectForLeafletModel>();
        }

        public Polygon GetBBox(Coordinate southWestPoint, Coordinate northEastPoint)
        {
            Coordinate[] imageOutlineCoordinates = new Coordinate[]
            {
                new Coordinate(southWestPoint.X, northEastPoint.Y),
                new Coordinate(northEastPoint.X, northEastPoint.Y),
                new Coordinate(northEastPoint.X, southWestPoint.Y),
                new Coordinate(southWestPoint.X, southWestPoint.Y),
                new Coordinate(southWestPoint.X, northEastPoint.Y),
            };
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var bbox = geometryFactory.CreatePolygon(imageOutlineCoordinates);
            return bbox;

        }
    }
}
