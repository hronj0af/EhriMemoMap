using EhriMemoMap.Data;
using EhriMemoMap.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Services;

/// <summary>
/// Logika nad mapou
/// </summary>
public class MapLogicService(MemogisContext context)
{
    private readonly MemogisContext _context = context;

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

    public WelcomeDialogStatistics GetWelcomeDialogStatistics(string city)
    {
        var statistics = _context.MapStatistics.Where(a => a.City == city && a.Type.Contains("total") && a.DateFrom == null && a.DateTo == null).ToList();
        return new WelcomeDialogStatistics
        {
            Victims = statistics.FirstOrDefault(a => a.Type.Contains("victims"))?.Count,
            Incidents = statistics.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count,
            Interests = statistics.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count,
            Inaccessibles = statistics.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count,
            PlacesOfMemory = statistics.FirstOrDefault(a => a.Type.Contains("places_of_memory"))?.Count
        };
    }

    public PlacesResult GetPlaces(PlacesParameters parameters)
    {
        var result = new PlacesResult
        {
            Incidents = GetIncidents(parameters),
            PlacesOfInterest = GetPlacesOfInterest(parameters),
            InaccessiblePlaces = GetInaccessiblePlaces(parameters),
            Addresses = GetAddressesWithVictims(parameters),
            PlacesOfMemory = GetPlacesOfMemories(parameters)
        };



        return result;
    }

    public List<PraguePlacesOfMemory>? GetPlacesOfMemories(PlacesParameters parameters)
    {
        if (parameters.PlacesOfMemoryIds == null)
            return null;

        return _context.PraguePlacesOfMemories.Where(p => parameters.PlacesOfMemoryIds.Contains(p.Id)).ToList();
    }

    public List<PragueIncidentsTimeline>? GetIncidents(PlacesParameters parameters)
    {
        if (parameters.IncidentsIds == null || parameters.City != "prague")
            return null;

        var result = new List<PragueIncidentsTimeline>();
        result = _context.PragueIncidentsTimelines.Where(p => parameters.IncidentsIds.Contains(p.Id)).ToList();

        // protože v databázi neexistuje vazba mezi incidentem a dokumentem, musíme si ji vytvořit ručně
        // tabulka prague_incident_x_documents obsahuje sloupec incident_id, 
        // který je vazbou na tabulku prague_incidents_timelines, 
        // jenže v ní je sloupec id, který není primárním klíčem
        // databázi spravuje Aneta Plzáková, nikoli já - takže to prozatím necháme takto
        foreach (var incident in result)
            incident.Documents = _context.PragueIncidentsXDocuments.Where(a => a.IncidentId == incident.Id).ToList();

        return result;
    }

    public List<PlaceInterest>? GetPlacesOfInterest(PlacesParameters parameters)
    {
        if (parameters.PlacesOfInterestIds == null)
            return null;

        if (parameters.City == "prague")
            return _context.PraguePlacesOfInterestTimelines.
                Where(p => parameters.PlacesOfInterestIds.Contains(p.Id)).
                Select(a => new PlaceInterest
                {
                    AddressCs = a.AddressCs,
                    AddressEn = a.AddressEn,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn,
                    DescriptionCs = a.DescriptionCs,
                    DescriptionEn = a.DescriptionEn,
                }).
                ToList();

        else if (parameters.City == "pacov")
            return _context.PacovPois.Include(a => a.Place).
                AsNoTracking().
                Where(p => parameters.PlacesOfInterestIds.Contains(p.Id)).
                Select(a => new PlaceInterest
                {
                    AddressCs = a.Place.StreetCs,
                    AddressEn = a.Place.StreetEn,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn,
                    DescriptionCs = a.DescriptionCs,
                    DescriptionEn = a.DescriptionEn,
                }).
                ToList();
        return null;

    }

    public List<PlaceInterest>? GetInaccessiblePlaces(PlacesParameters parameters)
    {
        if (parameters.InaccessiblePlacesIds == null)
            return null;

        if (parameters.City == "prague")
            return _context.PraguePlacesOfInterestTimelines.
            Where(p => parameters.InaccessiblePlacesIds.Contains(p.Id)).
            Select(a => new PlaceInterest
            {
                AddressCs = a.AddressCs,
                AddressEn = a.AddressEn,
                LabelCs = a.LabelCs,
                LabelEn = a.LabelEn,
                DescriptionCs = a.DescriptionCs,
                DescriptionEn = a.DescriptionEn,
            }).
            ToList();
        else if (parameters.City == "pacov")
        { }
        return null;

    }

    public List<AddressWithVictims>? GetAddressesWithVictims(PlacesParameters parameters)
    {
        if (parameters.AddressesIds == null)
            return null;

        if (parameters.City == "prague")
        {
            return _context.PragueAddressesStatsTimelines.Where(p => parameters.AddressesIds.Contains(p.Id)).
                Select(a => new AddressWithVictims
                {
                    Address = new AddressInfo
                    {
                        Cs = a.AddressCs,
                        En = a.AddressEn,
                        De = a.AddressDe,
                        CurrentCs = a.AddressCurrentCs,
                        CurrentEn = a.AddressCurrentEn,
                    },
                    PragueAddress = a,
                    Victims = _context.PragueVictimsTimelines.Where(b => b.PlaceId == a.Id).OrderBy(a => a.Label).
                        Select(a => new VictimShortInfo
                        {
                            DetailsCs = a.DetailsCs,
                            DetailsEn = a.DetailsEn,
                            Label = a.Label,
                            Photo = a.Photo,
                            TransportDate = a.TransportDate,
                            LongInfo = false
                        }).
                        ToList()
                }).
                ToList();
        }
        else if (parameters.City == "pacov")
        {
            return _context.PacovPlaces.
                Include(a => a.PacovEntitiesXPlaces).
                ThenInclude(a => a.Entity).
                ThenInclude(a => a.PacovEntitiesXMedia).
                ThenInclude(a => a.Media).
                Where(p => parameters.AddressesIds.Contains(p.Id)).
                AsEnumerable().
                Select(a => new AddressWithVictims
                {
                    Address = new AddressInfo
                    {
                        Cs = a.LabelCs,
                        En = a.LabelEn,
                    },
                    Victims = a.PacovEntitiesXPlaces.Where(a => a.RelationshipType == 26).Select(b => b.Entity).Distinct().Select(b => new VictimShortInfo
                    {
                        LongInfo = true,
                        Photo = b?.PacovEntitiesXMedia?.Select(c => c.Media)?.FirstOrDefault()?.OmekaUrl,
                        Label = b?.Surname + ", " + b?.Firstname + (b?.Birthdate != null ? " (*" + b?.Birthdate?.ToString("d.M.yyyy") + ")" : "")
                    }).ToList()
                }).
                ToList();
        }
        return null;

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
