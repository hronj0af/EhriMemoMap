using EhriMemoMap.Data;
using EhriMemoMap.Shared;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Services
{
    /// <summary>
    /// Logika nad mapou
    /// </summary>
    public class MapLogicService
    {
        private readonly MemogisContext _context;


        public MapLogicService(MemogisContext context)
        {
            _context = context;
        }

        public List<MapObject> GetMapObjects(MapObjectParameters parameters)
        {
            // nejdriv si pripravim mapove objekty pro dalsi dotazy
            var query = _context.MapObjects.AsQueryable();

            // vyfiltruju objekty podle toho, na jakem bode casove ose lezi
            if (parameters.SelectedTimeLinePoint != null)
            {
                // bude vyberu vsechny objekty, ktere nemaji prirazeno zadne datum
                if (parameters.SelectedTimeLinePoint.From == null && parameters.SelectedTimeLinePoint.To == null)
                    query = query.Where(a => a.DateFrom == null && a.DateTo == null);

                // anebo vyberu objekty, ktere lezi v casovem intervalu daneho bodu casove osy
                else if (parameters.CustomCoordinates == null)
                    query = query.Where(a => a.DateFrom >= parameters.SelectedTimeLinePoint.From && a.DateTo <= parameters.SelectedTimeLinePoint.To);

                // v případě, že je zadán parametr customCoordinates, tedy když se hledají objekty na mapě v místě, kam uživatel klikl,
                // pak k tomu přidám i vrstvu s typem "polygons", protože ty nemají datum a zobrazují se na mapě vždy
                else if (parameters.CustomCoordinates != null)
                    query = query.Where(a => (a.DateFrom >= parameters.SelectedTimeLinePoint.From && a.DateTo <= parameters.SelectedTimeLinePoint.To) || (a.PlaceType == "Inaccessible"));
            }

            if (parameters.SelectedLayerNames != null)
                query = query.Where(a => a.PlaceType != null && parameters.SelectedLayerNames.Contains(a.PlaceType));

            // a nakonec vyberu ty objekty, ktere jsou viditelne na zobrazenem vyrezu na mape
            if (parameters.CustomCoordinates != null && parameters.CustomCoordinates.Length == 2)
            {
                var bbox = GetBBox(parameters.CustomCoordinates[0], parameters.CustomCoordinates[1]);
                query = query.Where(a => (a.GeographyMapPoint != null && bbox.Intersects(a.GeographyMapPoint)) || (a.GeographyMapPolygon != null && bbox.Intersects(a.GeographyMapPolygon)));
            }
            else if (parameters.MapSouthWestPoint != null && parameters.MapNorthEastPoint != null)
            {
                var bbox = GetBBox(parameters.MapNorthEastPoint, parameters.MapSouthWestPoint);
                query = query.Where(a => !string.IsNullOrEmpty(a.MapPolygon) || (a.GeographyMapPoint != null && a.GeographyMapPoint.Intersects(bbox)));
            }

            var result = query.ToList();

            return result;
        }

        public List<MapStatistic> GetDistrictStatistics(DistrictStatisticsParameters parameters)
        {
            var result = _context.MapStatistics.
                Where(a => (parameters.Total ? a.Type.Contains("total") : !a.Type.Contains("total")) && a.DateFrom == parameters.TimeLinePoint).ToList();
            return result;
        }

        public WelcomeDialogStatistics GetWelcomeDialogStatistics()
        {
            var statistics = _context.MapStatistics.Where(a => a.Type.Contains("total") && a.DateFrom == null && a.DateTo == null).ToList();
            return new WelcomeDialogStatistics
            {
                Victims = statistics.FirstOrDefault(a => a.Type.Contains("victims"))?.Count,
                Incidents = statistics.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count,
                Interests = statistics.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count,
                Inaccessibles = statistics.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count
            };
        }

        public PlacesResult GetPlaces(PlacesParameters parameters) 
        {
            var result = new PlacesResult();
            if (parameters.IncidentsIds != null)
            {
                result.Incidents = _context.PragueIncidentsTimelines.Where(p => parameters.IncidentsIds.Contains(p.Id)).ToList();

                // protože v databázi neexistuje vazba mezi incidentem a dokumentem, musíme si ji vytvořit ručně
                // tabulka prague_incident_x_documents obsahuje sloupec incident_id, 
                // který je vazbou na tabulku prague_incidents_timelines, 
                // jenže v ní je sloupec id, který není primárním klíčem
                // databázi spravuje Aneta Plzáková, nikoli já - takže to prozatím necháme takto
                foreach (var incident in result.Incidents)
                    incident.Documents = _context.PragueIncidentsXDocuments.Where(a => a.IncidentId == incident.Id).ToList();
            }

            if (parameters.PlacesOfInterestIds != null)
                result.PlacesOfInterest = _context.PraguePlacesOfInterestTimelines.Where(p => parameters.PlacesOfInterestIds.Contains(p.Id)).ToList();

            if (parameters.InaccessiblePlacesIds != null)
                result.InaccessiblePlaces = _context.PraguePlacesOfInterestTimelines.Where(p => parameters.InaccessiblePlacesIds.Contains(p.Id)).ToList();

            if (parameters.AddressesIds != null)
                result.Addresses = _context.PragueAddressesStatsTimelines.Where(p => parameters.AddressesIds.Contains(p.Id)).
                    Select(a => new AddressWithVictimsWrappwer
                    {
                        Address = a,
                        Victims = _context.PragueVictimsTimelines.Where(b => b.PlaceId == a.Id).OrderBy(a => a.Label).ToList()
                    }).
                    ToList();

            return result;
        }

        private Polygon GetBBox(PointModel southWestPoint, PointModel northEastPoint)
        {
            var imageOutlineCoordinates = new Coordinate[]
            {
                new(southWestPoint.X, northEastPoint.Y),
                new(northEastPoint.X, northEastPoint.Y),
                new(northEastPoint.X, southWestPoint.Y),
                new(southWestPoint.X, southWestPoint.Y),
                new(southWestPoint.X, northEastPoint.Y),
            };
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var bbox = geometryFactory.CreatePolygon(imageOutlineCoordinates);
            return bbox;

        }
    }
}
