using EhriMemoMap.Data;
using EhriMemoMap.Models;
using NetTopologySuite.Geometries;
using Radzen;

namespace EhriMemoMap.Services
{
    /// <summary>
    /// Logika nad mapou
    /// </summary>
    public class MapLogicService
    {
        private readonly MapStateService _mapState;
        private readonly MemogisContext _context;

        public MapLogicService(MapStateService mapState, MemogisContext context)
        {
            _mapState = mapState;
            _context = context;
        }

        public List<MapObjectForLeaflet> GetMapObjects()
        {
            if (_mapState == null || _mapState.Map == null)
                return new List<MapObjectForLeaflet>();
            
            // nejdriv si pripravim mapove objekty pro dalsi dotazy
            var result = _context.MapObjects.AsQueryable();

            // vyfiltruju objekty podle toho, na jakem bode casove ose lezi
            if (_mapState.Map.Timeline != null && _mapState.Map.Timeline.Any(a => a.Selected))
            {
                var timelimePoint = _mapState.Map.Timeline.FirstOrDefault(a => a.Selected);
                
                // bude vyberu vsechny objekty, ktere nemaji prirazeno zadne datum
                if (timelimePoint != null && timelimePoint.From == null && timelimePoint.To == null)
                    result = result.Where(a => a.DateFrom == null && a.DateTo == null);
                
                // anebo vyberu objekty, ktere lezi v casovem intervalu daneho bodu casove osy
                else if (timelimePoint != null)
                    result = result.Where(a => a.DateFrom >= timelimePoint.From && a.DateTo <= timelimePoint.To);
            }

            // vyber objektu podle toho, do jake nalezi vrstvy
            var selectedLayerNames = new List<string>();
            foreach (var layer in _mapState.GetNotBaseLayers(true))
            {
                // krome nazvu vrstvy zkoumam i to, jestli se pri danem zoomu mapy ma vrstva vubec zobrazovat
                if (string.IsNullOrEmpty(layer.Name) || (layer.MinZoom != null && _mapState.MapZoom < layer.MinZoom) || (layer.MaxZoom != null && _mapState.MapZoom > layer.MaxZoom))
                    continue;
                selectedLayerNames.Add(layer.Name);
            }

            result = result.Where(a => !string.IsNullOrEmpty(a.PlaceType) && selectedLayerNames.Contains(a.PlaceType));

            // a nakonec vyberu ty objekty, ktere jsou viditelne na zobrazenem vyrezu na mape
            var bbox = new Envelope(_mapState.MapSouthWestPoint, _mapState.MapNorthEastPoint);

            // TODO - zrejme se nebudou zobrazovat polygony, jejichz uvedeny stred nelezi na mape
            result = result.Where(a => a.MapLocation != null && bbox.Covers(a.MapLocation.Coordinate.X, a.MapLocation.Coordinate.Y));

            return result.Select(a => new MapObjectForLeaflet(a)).ToList();
        }
    }
}
