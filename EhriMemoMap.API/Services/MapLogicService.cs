﻿using EhriMemoMap.Data;
using EhriMemoMap.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using EhriMemoMap.API.Helpers;
using Newtonsoft.Json;

namespace EhriMemoMap.Services;

/// <summary>
/// Logika nad mapou
/// </summary>
public class MapLogicService(MemogisContext context)
{
    private readonly MemogisContext _context = context;

    public IQueryable<MapObject> PrepareMapObjectsQuery(MapObjectParameters parameters)
    {
        // nejdriv si pripravim mapove objekty pro dalsi dotazy
        var query = _context.MapObjects.Where(a => a.City == parameters.City).AsQueryable();

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
            NormalizeCustomCoordinate(parameters);
            var bbox = GetBBox(parameters.CustomCoordinates[0], parameters.CustomCoordinates[1]);
            query = query.Where(a => (a.GeographyMapPoint != null && bbox.Intersects(a.GeographyMapPoint)) || (a.GeographyMapPolygon != null && bbox.Intersects(a.GeographyMapPolygon)));
        }
        else if (parameters.MapSouthWestPoint != null && parameters.MapNorthEastPoint != null)
        {
            var bbox = GetBBox(parameters.MapNorthEastPoint, parameters.MapSouthWestPoint);
            query = query.Where(a => !string.IsNullOrEmpty(a.MapPolygon) || (a.GeographyMapPoint != null && a.GeographyMapPoint.Intersects(bbox)));
        }

        return query;
    }

    private static void NormalizeCustomCoordinate(MapObjectParameters? parameters)
    {
        if (parameters?.CustomCoordinates == null || parameters.CustomCoordinates.Length != 2)
            return;

        var point1 = parameters.CustomCoordinates[0];
        var point2 = parameters.CustomCoordinates[1];

        // Kontrola, zda body nejsou totožné
        if (point1.X == point2.X && point1.Y == point2.Y)
        {
            // Pokud jsou body totožné, mírně je posuneme, aby vznikl malý čtverec
            double offset = 0.0001; // Velikost posunutí (upravte dle potřeby)
            point1.X += offset;
            point1.X += offset;
            point2.X -= offset;
            point2.Y -= offset;
        }

        // Vytvoření geometrie z bodů
        var geometry1 = new PointModel { X = point1.X, Y = point1.Y };
        var geometry2 = new PointModel { X = point2.X, Y = point2.Y };

        parameters.CustomCoordinates = [geometry1, geometry2];

    }

    public MapObject[] GetMapObjects(MapObjectParameters parameters)
    {
        var result = PrepareMapObjectsQuery(parameters);
        Console.WriteLine(JsonConvert.SerializeObject(parameters));
        var temp = result.ToList();
        return [.. result];
    }

    public MapObject[] GetHeatmap(MapObjectParameters parameters)
    {
        var result = PrepareMapObjectsQuery(parameters).Select(a => new MapObject
        {
            MapPoint = a.MapPoint,
            Citizens = a.Citizens,
            PlaceType = a.PlaceType
        }).ToArray();
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
            AddressesLastResidence = GetAddressesLastResidenceWithVictims(parameters),
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

    public List<PlaceIncident>? GetIncidents(PlacesParameters parameters)
    {
        if (parameters.IncidentsIds == null)
            return null;

        var result = new List<PlaceIncident>();

        if (parameters.City?.Contains("prague") ?? false)
        {
            result = _context.PragueIncidentsTimelines.Where(p => parameters.IncidentsIds.Contains(p.Id)).
            Select(a => new PlaceIncident
            {
                Id = a.Id,
                Date = a.DateIso,
                DescriptionCs = a.DescriptionCs,
                DescriptionEn = a.DescriptionEn,
                LabelCs = a.LabelCs,
                LabelEn = a.LabelEn,
                SpecificationCs = a.Spec1Cs,
                SpecificationEn = a.Spec1En,
                TypeCs = a.Type1Cs,
                TypeEn = a.Type1En,
                AddressCs = a.PlaceCs,
                AddressEn = a.PlaceEn,
            }).
            ToList();

            // protože v databázi neexistuje vazba mezi incidentem a dokumentem, musíme si ji vytvořit ručně
            // tabulka prague_incident_x_documents obsahuje sloupec incident_id, 
            // který je vazbou na tabulku prague_incidents_timelines, 
            // jenže v ní je sloupec id, který není primárním klíčem
            // databázi spravuje Aneta Plzáková, nikoli já - takže to prozatím necháme takto
            foreach (var incident in result)
                incident.Documents = _context.PragueIncidentsXDocuments.Where(a => a.IncidentId == incident.Id).
                    Select(a => new Document
                    {
                        DocumentUrlCs = a.DocumentCs,
                        DocumentUrlEn = a.DocumentEn,
                        Url = new string[] { a.Img }
                    }).
                    ToArray();
        }
        else if (parameters.City?.Contains("pacov") ?? false)
        {
            result = _context.PacovIncidents.
                Include(a => a.Place).
                Where(p => parameters.IncidentsIds.Contains(p.Id)).
                AsEnumerable().
                Select(a => new PlaceIncident
                {
                    DateCs = a.DateCs,
                    DateEn = a.DateEn,
                    DescriptionCs = a.DescriptionCs,
                    DescriptionEn = a.DescriptionEn,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn,
                    AddressCs = a.Place?.LabelCs,
                    AddressEn = a.Place?.LabelEn,
                }).ToList();
        }


        return result;
    }

    public List<PlaceInterest>? GetPlacesOfInterest(PlacesParameters parameters)
    {
        if (parameters.PlacesOfInterestIds == null)
            return null;

        if (parameters.City?.Contains("prague") ?? false)
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
            return _context.PacovPois.
                Include(a => a.Place).
                Include(a => a.PacovDocumentsXPois).ThenInclude(a => a.Document).ThenInclude(a => a.PacovDocumentsXMedia).ThenInclude(a => a.Medium).
                AsNoTracking().
                AsEnumerable().
                Where(p => parameters.PlacesOfInterestIds.Contains(p.Id)).
                Select(a => new PlaceInterest
                {
                    AddressCs = a.Place.LabelCs,
                    AddressEn = a.Place.LabelEn,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn,
                    DescriptionCs = a.DescriptionCs,
                    DescriptionEn = a.DescriptionEn,
                    Documents = a.PacovDocumentsXPois.Select(b => b.Document).Select(c => new Document
                    {
                        CreationDateCs = c.CreationDateCs,
                        CreationDateEn = c.CreationDateEn,
                        DescriptionCs = c.DescriptionCs,
                        DescriptionEn = c.DescriptionEn,
                        LabelCs = c.LabelCs,
                        LabelEn = c.LabelEn,
                        CreationPlaceCs = c.CreationPlaceNavigation?.LabelCs,
                        CreationPlaceEn = c.CreationPlaceNavigation?.LabelEn,
                        Id = c.Id,
                        Owner = c.Owner,
                        Type = c.Type,
                        Url = c?.PacovDocumentsXMedia?.Select(d => d?.Medium?.OmekaUrl)?.ToArray() ?? []
                    }).ToArray()
                }).
                ToList();
        return null;

    }

    public List<PlaceInterest>? GetInaccessiblePlaces(PlacesParameters parameters)
    {
        if (parameters.InaccessiblePlacesIds == null)
            return null;

        if (parameters.City?.Contains("prague") ?? false)
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

        if (parameters.City?.Contains("prague") ?? false)
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
                        Select(a => new VictimShortInfoModel
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
                Include(a => a.PacovEntitiesXPlaces).ThenInclude(a => a.Entity).ThenInclude(a => a.PacovEntitiesXMedia).ThenInclude(a => a.Medium).
                Include(a => a.PacovEntitiesXPlaces).ThenInclude(a => a.RelationshipTypeNavigation).
                Include(a => a.PacovEntitiesXPlaces).ThenInclude(a => a.Entity).ThenInclude(a => a.PacovEntitiesXNarrativeMaps).
                Where(p => parameters.AddressesIds.Contains(p.Id)).
                AsEnumerable().
                Select(a => new AddressWithVictims
                {
                    Address = new AddressInfo
                    {
                        Cs = a.LabelCs,
                        En = a.LabelEn,
                    },
                    Victims = a.PacovEntitiesXPlaces.Select(b => new VictimShortInfoModel
                    {
                        Id = b?.Entity.Id ?? 0,
                        LongInfo = true,
                        Photo = b?.Entity.PacovEntitiesXMedia?.Select(c => c.Medium)?.FirstOrDefault()?.OmekaUrl,
                        Label = b?.Entity.Surname + ", " + b?.Entity.Firstname + (b?.Entity.Birthdate != null ? " (*" + b?.Entity.Birthdate?.ToString("d.M.yyyy") + ")" : ""),
                        RelationshipToAddressType = b?.RelationshipType,
                        NarrativeMapId = b?.Entity.PacovEntitiesXNarrativeMaps.FirstOrDefault()?.NarrativeMapId,
                    }).ToList()
                }).
                ToList();
        }
        return null;

    }

    public List<AddressWithVictims>? GetAddressesLastResidenceWithVictims(PlacesParameters parameters)
    {
        if (parameters.AddressesLastResidenceIds == null)
            return null;

        if (parameters.City != "prague_last_residence")
            return null;

        return _context.PragueAddressesStatsTimelines.Where(p => parameters.AddressesLastResidenceIds.Contains(p.Id)).
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
                Victims = _context.PragueVictimsTimelines.Where(b => b.PragueLastResidences.Any(c => c.AddressId == a.Id)).OrderBy(a => a.Label).
                    Select(a => new VictimShortInfoModel
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

    public VictimLongInfoModel? GetVictimLongInfo(string city, long id)
    {
        if (city.Contains("prague"))
            return null;

        var result = _context.PacovEntities.
            Include(a => a.FateNavigation).
            Include(a => a.PacovEntitiesXNarrativeMaps).
            Include(a => a.PacovEntitiesXTransports).ThenInclude(a => a.Transport).ThenInclude(a => a.PlaceFromNavigation).
            Include(a => a.PacovEntitiesXTransports).ThenInclude(a => a.Transport).ThenInclude(a => a.PlaceToNavigation).
            Include(a => a.PacovEntitiesXMedia).ThenInclude(a => a.Medium).
            Include(a => a.PacovEntitiesXPlaces).ThenInclude(a => a.Place).
            Include(a => a.PacovEntitiesXPlaces).ThenInclude(a => a.RelationshipTypeNavigation).
            Include(a => a.PacovEntitiesXEntityEntity2s).ThenInclude(a => a.Entity1).ThenInclude(a => a.PacovEntitiesXMedia).ThenInclude(a => a.Medium).
            Include(a => a.PacovEntitiesXEntityEntity2s).ThenInclude(a => a.Entity1).ThenInclude(a => a.PacovEntitiesXNarrativeMaps).
            Include(a => a.PacovEntitiesXEntityEntity2s).ThenInclude(a => a.RelationshipTypeNavigation).
            Include(a => a.PacovDocumentsXEntities).ThenInclude(a => a.Document).ThenInclude(a => a.PacovDocumentsXMedia).ThenInclude(a => a.Medium).
            Include(a => a.PacovDocumentsXEntities).ThenInclude(a => a.Document).ThenInclude(a => a.CreationPlaceNavigation).
            Where(a => a.Id == id).
            AsEnumerable().
            Select(b => new VictimLongInfoModel
            {
                Id = b.Id,
                NarrativeMapId = b.PacovEntitiesXNarrativeMaps.FirstOrDefault()?.NarrativeMapId,
                BirthDate = b.Birthdate,
                DeathDate = b.Deathdate,
                Label = b?.Surname + ", " + b?.Firstname + (b?.Birthdate != null ? " (*" + b?.Birthdate?.ToString("d.M.yyyy") + ")" : ""),
                FateCs = b?.Sex == 3 ? b?.FateNavigation?.LabelCs?.Replace("/", "") : b?.FateNavigation?.LabelCs?.Replace("/a", ""),
                FateEn = b?.FateNavigation?.LabelEn,
                Photo = b?.PacovEntitiesXMedia.Select(a => a.Medium).FirstOrDefault()?.OmekaUrl,
                Places = b?.PacovEntitiesXPlaces.Select(a => new AddressInfo
                {
                    Cs = a.Place.LabelCs,
                    En = a.Place.LabelEn ?? a.Place.LabelCs,
                    Type = a.RelationshipType,
                    TypeCs = a.RelationshipTypeNavigation.LabelCs,
                    TypeEn = a.RelationshipTypeNavigation.LabelEn
                }).ToArray(),
                Documents = b?.PacovDocumentsXEntities.Select(c => c.Document).Select(c => new Document
                {
                    CreationDateCs = c.CreationDateCs,
                    CreationDateEn = c.CreationDateEn,
                    DescriptionCs = c.DescriptionCs,
                    DescriptionEn = c.DescriptionEn,
                    LabelCs = c.LabelCs,
                    LabelEn = c.LabelEn,
                    CreationPlaceCs = c.CreationPlaceNavigation?.LabelCs,
                    CreationPlaceEn = c.CreationPlaceNavigation?.LabelEn,
                    Id = c.Id,
                    Owner = c.Owner,
                    Type = c.Type,
                    Url = c?.PacovDocumentsXMedia?.Select(d => d?.Medium?.OmekaUrl)?.ToArray() ?? []
                }).ToArray(),
                RelatedPersons = b?.PacovEntitiesXEntityEntity2s.Select(a => new VictimShortInfoModel
                {
                    Id = a.Entity1.Id,
                    Name = a.Entity1.Surname + ", " + a.Entity1.Firstname,
                    Birthdate = a.Entity1.Birthdate?.ToString("d.M.yyyy"),
                    Photo = a.Entity1.PacovEntitiesXMedia.Select(c => c.Medium).FirstOrDefault()?.OmekaUrl,
                    RelationshipToPersonCs = a.RelationshipTypeNavigation.LabelCs,
                    RelationshipToPersonEn = a.RelationshipTypeNavigation.LabelEn,
                    RelationshipToPersonType = a.RelationshipType,
                    LongInfo = true,
                    NarrativeMapId = a?.Entity1.PacovEntitiesXNarrativeMaps.FirstOrDefault()?.NarrativeMapId,
                }).ToArray(),
                Transports = b?.PacovEntitiesXTransports.Select(a => new Transport
                {
                    Date = a.Transport.Date,
                    FromCs = a.Transport?.PlaceFromNavigation?.LabelCs,
                    FromEn = a.Transport?.PlaceFromNavigation?.LabelEn,
                    ToCs = a.Transport?.PlaceToNavigation?.LabelCs,
                    ToEn = a.Transport?.PlaceToNavigation?.LabelEn,
                    Code = a.Transport?.TransportCode,
                }).ToArray(),
            }).
            FirstOrDefault();
        return result;
    }

    public List<NarrativeMap> GetAllNarrativeMaps(string city)
    {
        var result = _context.PacovNarrativeMaps.
            Select(a => new NarrativeMap { Id = a.Id, LabelCs = a.LabelCs, LabelEn = a.LabelEn }).ToList();
        return result;
    }
    public NarrativeMap? GetNarrativeMap(long id)
    {
        var map = _context.PacovNarrativeMaps.
            Include(a => a.PacovEntitiesXNarrativeMaps).ThenInclude(a => a.Entity).ThenInclude(a => a.PacovEntitiesXMedia).ThenInclude(a => a.Medium).
            Include(a => a.PacovNarrativeMapsXNarrativeMapStops).
            Select(a => new NarrativeMap
            {
                Id = a.Id,
                LabelCs = a.LabelCs,
                LabelEn = a.LabelEn,
                DescriptionCs = a.DescriptionCs,
                DescriptionEn = a.DescriptionEn,
                Type = a.Type
            }).
            FirstOrDefault(a => a.Id == id);

        if (map == null)
            return null;

        var stops = _context.PacovNarrativeMapStops.
            Include(a => a.PacovNarrativeMapStopsXPlaces).ThenInclude(a => a.Place).
            Include(a => a.PacovNarrativeMapStopsXPlaces).ThenInclude(a => a.RelationshipTypeNavigation).
            Include(a => a.PacovDocumentsXNarrativeMapStops).ThenInclude(a => a.Document).ThenInclude(a => a.PacovDocumentsXMedia).ThenInclude(a => a.Medium).
            Where(a => a.PacovNarrativeMapXNarrativeMapStops.Any(b => b.NarrativeMapId == id)).
            AsEnumerable().
            Select(a => new NarrativeMapStop
            {
                Id = a.Id,
                LabelCs = a.LabelCs,
                LabelEn = a.LabelEn,
                DescriptionCs = a.DescriptionCs,
                DescriptionEn = a.DescriptionEn,
                Places = a.PacovNarrativeMapStopsXPlaces.Select(b => new Place
                {
                    Id = b.Id,
                    LabelCs = b.RelationshipTypeNavigation.LabelEn == "main point" ? a.LabelCs + "<br/>" + b.Place.LabelCs : b.Place.LabelCs,
                    LabelEn = b.RelationshipTypeNavigation.LabelEn == "main point" ? a.LabelEn + "<br/>" + b.Place.LabelEn : b.Place.LabelEn,
                    TownCs = b.Place.TownCs,
                    TownEn = b.Place.TownEn,
                    StreetCs = b.Place.StreetCs,
                    StreetEn = b.Place.StreetEn,
                    HouseNr = b.Place.HouseNr,
                    RemarkCs = b.Place.RemarkCs,
                    RemarkEn = b.Place.RemarkEn,
                    MapPoint = b.Place.Geography.AsJson(),
                    Type = b.RelationshipTypeNavigation.LabelEn,
                    StopId = b.NarrativeMapStopId
                }).ToArray(),
                Documents = a.PacovDocumentsXNarrativeMapStops.Select(b => b.Document).Select(b => new Document
                {
                    CreationDateCs = b.CreationDateCs,
                    CreationDateEn = b.CreationDateEn,
                    DescriptionCs = b.DescriptionCs,
                    DescriptionEn = b.DescriptionEn,
                    LabelCs = b.LabelCs,
                    LabelEn = b.LabelEn,
                    CreationPlaceCs = b!.CreationPlaceNavigation?.LabelCs,
                    CreationPlaceEn = b!.CreationPlaceNavigation?.LabelEn,
                    Id = b!.Id,
                    Owner = b.Owner,
                    Type = b.Type,
                    Url = b?.PacovDocumentsXMedia?.Select(c => c?.Medium?.OmekaUrl)?.ToArray() ?? []
                }).ToArray()
            }).ToArray();

        map.Stops = stops;

        return map;
    }
}
