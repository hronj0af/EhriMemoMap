using EhriMemoMap.Data;
using EhriMemoMap.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using EhriMemoMap.API.Helpers;
using Newtonsoft.Json;
using EhriMemoMap.Data.MemoMap;
using EhriMemoMap.API.Services;

namespace EhriMemoMap.Services;

/// <summary>
/// Logika nad mapou
/// </summary>
public class MapLogicService(MemogisContext context, MemoMapContextFactory factory)
{
    public IQueryable<Data.MapObject> PrepareMapObjectsQuery(MapObjectParameters parameters)
    {
        // nejdriv si pripravim mapove objekty pro dalsi dotazy
        var query = context.MapObjects.Where(a => a.City == parameters.City).AsQueryable();

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

    public List<Data.MemoMap.MapObject> PrepareMapObjectsQueryMemoMap(MapObjectParameters parameters)
    {
        using var context = factory.GetContext(parameters.City);
        // nejdriv si pripravim mapove objekty pro dalsi dotazy
        var query = context.MapObjects.AsQueryable();

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

        return query.ToList();
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

    public Shared.MapObject[] GetMapObjects(MapObjectParameters parameters)
    {
        Console.WriteLine(JsonConvert.SerializeObject(parameters));

        if (parameters.City.IsMemoMapCity())
        {
            var resultMemoMap = PrepareMapObjectsQueryMemoMap(parameters);
            return [.. resultMemoMap.Select(a => a.ConvertToMapObjectShared())];
        }

        var result = PrepareMapObjectsQuery(parameters);
        return [.. result.Select(a => a.ConvertToMapObjectShared())];
    }

    public Shared.MapObject[] GetHeatmap(MapObjectParameters parameters)
    {
        var result = PrepareMapObjectsQuery(parameters).Select(a => new Shared.MapObject
        {
            MapPoint = a.MapPoint,
            Citizens = a.Citizens,
            PlaceType = a.PlaceType
        }).ToArray();
        return result;
    }

    public List<Shared.MapStatistic> GetDistrictStatistics(DistrictStatisticsParameters parameters)
    {
        if (parameters.City.IsMemoMapCity())
        {
            var context = factory.GetContext(parameters.City);
            var resultMemoMap = context.MapStatistics.
                Where(a => (parameters.Total ? a.Type.Contains("total") : !a.Type.Contains("total")) && a.DateFrom == parameters.TimeLinePoint).ToList();
            return resultMemoMap.Select(a => a.ConvertToSharedStatistic()).ToList();
        }

        var result = context.MapStatistics.
            Where(a => (parameters.Total ? a.Type.Contains("total") : !a.Type.Contains("total")) && a.DateFrom == parameters.TimeLinePoint).ToList();
        return result.Select(a => a.ConvertToSharedStatistic()).ToList();
    }

    public WelcomeDialogStatistics GetWelcomeDialogStatistics(string city)
    {
        if (city.IsMemoMapCity())
        {
            var context = factory.GetContext(city);
            var statisticsMemoMap = context.MapStatistics.Where(a => a.Type.Contains("total") && a.DateFrom == null && a.DateTo == null).ToList();
            return new WelcomeDialogStatistics
            {
                Victims = statisticsMemoMap.FirstOrDefault(a => a.Type.Contains("victims"))?.Count,
                Incidents = statisticsMemoMap.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count,
                Interests = statisticsMemoMap.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count,
                Inaccessibles = statisticsMemoMap.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count,
                PlacesOfMemory = statisticsMemoMap.FirstOrDefault(a => a.Type.Contains("places_of_memory"))?.Count,
                Memorials = statisticsMemoMap.FirstOrDefault(a => a.Type.Contains("memorials"))?.Count,
                StoryMaps = statisticsMemoMap.FirstOrDefault(a => a.Type.Contains("narrative_maps"))?.Count,
            };
        }

        var statistics = context.MapStatistics.Where(a => a.City == city && a.Type.Contains("total") && a.DateFrom == null && a.DateTo == null).ToList();
        return new WelcomeDialogStatistics
        {
            Victims = statistics.FirstOrDefault(a => a.Type.Contains("victims"))?.Count,
            Incidents = statistics.FirstOrDefault(a => a.Type.Contains("incidents"))?.Count,
            Interests = statistics.FirstOrDefault(a => a.Type.Contains("pois_points"))?.Count,
            Inaccessibles = statistics.FirstOrDefault(a => a.Type.Contains("pois_polygons"))?.Count,
            PlacesOfMemory = statistics.FirstOrDefault(a => a.Type.Contains("places_of_memory"))?.Count,
            Memorials = statistics.FirstOrDefault(a => a.Type.Contains("memorials"))?.Count,
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
            PlacesOfMemory = GetPlacesOfMemories(parameters),
            Memorials = GetMemorials(parameters)
        };
        return result;
    }

    public List<PlaceMemory>? GetPlacesOfMemories(PlacesParameters parameters)
    {
        if (parameters.City == "prague")
            return GetPlacesOfMemoriesPrague(parameters);
        else if (parameters.City.IsMemoMapCity())
            return GetPlacesOfMemoriesMemoMap(parameters);
        else
            return null;
    }

    public List<PlaceMemory>? GetPlacesOfMemoriesPrague(PlacesParameters parameters)
    {
        if (parameters.PlacesOfMemoryIds == null)
            return null;

        var result = context.PraguePlacesOfMemories.Where(p => parameters.PlacesOfMemoryIds.Contains(p.Id)).
            AsEnumerable().
            GroupBy(p => (p.Type, p.AddressCs)).Select(a => new PlaceMemory
            {
                Type = a.Key.Type,
                City = "prague",
                AddressCs = a.Key.AddressCs,
                AddressEn = a.FirstOrDefault()?.AddressEn,
                Items = a.Select(b => new PlaceMemoryItem
                {
                    Id = b.Id,
                    LabelCs = b.Label,
                    LabelEn = b.Label,
                    LinkStolpersteineCs = b.LinkStolpersteineCs,
                    LinkStolpersteineEn = b.LinkStolpersteineEn,
                    LinkHolocaustCs = b.LinkHolocaustCs,
                    LinkHolocaustEn = b.LinkHolocaustEn
                }).ToArray()
            });

        return [.. result];
    }

    public List<PlaceMemory>? GetPlacesOfMemoriesMemoMap(PlacesParameters parameters)
    {
        if (parameters.PlacesOfMemoryIds == null)
            return null;

        using var context = factory.GetContext(parameters.City);
        var result = context.PlacesOfMemories.
            Where(a => a.Type != "stolperstein").
            Include(a => a.PlacesXPlacesOfMemories).ThenInclude(a => a.Place).
            Include(a => a.PlacesOfMemoryXPlacesOfMemoryPlaceOfMemory2s).ThenInclude(a => a.PlaceOfMemory1).
            Include(a => a.DocumentsXPlacesOfMemories).ThenInclude(a => a.Document).ThenInclude(a => a.DocumentsXMedia).ThenInclude(a => a.Media).
            Where(p => parameters.PlacesOfMemoryIds.Contains(p.Id)).
            AsEnumerable().
            Select(a => new PlaceMemory
            {
                Type = a.Type,
                LabelCs = a.LabelCs,
                LabelEn = a.LabelEn,
                City = parameters.City,
                AddressCs = a.PlacesXPlacesOfMemories.FirstOrDefault(b => b.RelationshipType == 38)?.Place?.LabelCs,
                AddressEn = a.PlacesXPlacesOfMemories.FirstOrDefault(b => b.RelationshipType == 38)?.Place?.LabelEn,
                DescriptionCs = a.DescriptionCs,
                DescriptionEn = a.DescriptionEn,
                InscriptionCs = a.InscriptionCs,
                InscriptionEn = a.InscriptionEn,
                CreationDate = a.CreationDate,
                Items = a.PlacesOfMemoryXPlacesOfMemoryPlaceOfMemory2s.Select(b => b.PlaceOfMemory1).Select(b => new PlaceMemoryItem
                {
                    Id = b.Id,
                    LabelCs = b.LabelCs,
                    LabelEn = b.LabelEn,
                    InscriptionCs = b.InscriptionCs,
                    InscriptionEn = b.InscriptionEn,
                    CreationDate = b.CreationDate,
                }).ToArray(),
                Documents = a.DocumentsXPlacesOfMemories.Select(b => b.Document).Select(c => new Shared.Document
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
                    Url = c?.DocumentsXMedia?.Select(d => new OmekaUrl(d?.Media?.OmekaUrl, null)).ToArray() ?? []
                }).ToArray()
            });

        return [.. result];
    }

    public List<PlaceIncident>? GetIncidents(PlacesParameters parameters)
    {
        if (parameters.IncidentsIds == null)
            return null;

        var result = new List<PlaceIncident>();

        if (parameters.City?.Contains("prague") ?? false)
        {
            result = context.PragueIncidentsTimelines.Where(p => parameters.IncidentsIds.Contains(p.Id)).
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
                incident.Documents = context.PragueIncidentsXDocuments.Where(a => a.IncidentId == incident.Id).
                    Select(a => new Shared.Document
                    {
                        DocumentUrlCs = a.DocumentCs,
                        DocumentUrlEn = a.DocumentEn,
                        Url = new OmekaUrl[] { new(a.Img, null) }
                    }).
                    ToArray();
        }
        else if (parameters.City.IsMemoMapCity())
        {
            using var context = factory.GetContext(parameters.City);
            result = context.Incidents.
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
            return context.PraguePlacesOfInterestTimelines.
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

        else if (parameters.City.IsMemoMapCity())
        {
            using var context = factory.GetContext(parameters.City);
            return context.Pois.
                Include(a => a.Place).
                Include(a => a.DocumentsXPois).ThenInclude(a => a.Document).ThenInclude(a => a.DocumentsXMedia).ThenInclude(a => a.Media).
                Include(a => a.NarrativeMapsXPois).
                AsNoTracking().
                Where(p => parameters.PlacesOfInterestIds.Contains(p.Id)).
                AsEnumerable().
                Select(a => new PlaceInterest
                {
                    AddressCs = a.Place.LabelCs,
                    AddressEn = a.Place.LabelEn,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn,
                    DescriptionCs = a.DescriptionCs,
                    DescriptionEn = a.DescriptionEn,
                    NarrativeMapId = a.NarrativeMapsXPois.FirstOrDefault()?.NarrativeMapId,
                    Documents = a.DocumentsXPois.Select(b => b.Document).Select(c => new Shared.Document
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
                        Url = c?.DocumentsXMedia?.Select(d => new OmekaUrl(d?.Media?.OmekaUrl, d?.Media?.OmekaThumbnailUrl))?.ToArray() ?? []
                    }).ToArray()
                }).
                ToList();
        }
        return null;

    }

    public List<PlaceMemorial>? GetMemorials(PlacesParameters parameters)
    {
        if (parameters.MemorialsIds == null)
            return null;

        if (parameters.City.IsMemoMapCity())
        {
            using var context = factory.GetContext(parameters.City);
            return context.Events.
                Include(a => a.EventsXPlaces).ThenInclude(a => a.Place).
                Include(a => a.DocumentsXEvents).ThenInclude(a => a.Document).ThenInclude(a => a.DocumentsXMedia).ThenInclude(a => a.Media).
                AsNoTracking().
                AsEnumerable().
                Where(p => parameters.MemorialsIds.Contains(p.Id)).
                Select(a => new PlaceMemorial
                {
                    AddressCs = a.EventsXPlaces.FirstOrDefault()?.Place.LabelCs,
                    AddressEn = a.EventsXPlaces.FirstOrDefault()?.Place.LabelEn,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn,
                    LinkCs = a.LinkCs,
                    LinkEn = a.LinkEn,
                    DescriptionCs = a.DescriptionCs,
                    DescriptionEn = a.DescriptionEn,
                    Date = a.Date,
                    Documents = a.DocumentsXEvents.Select(b => b.Document).Select(c => new Shared.Document
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
                        Url = c?.DocumentsXMedia?.Select(d => new OmekaUrl(d?.Media?.OmekaUrl, d?.Media?.OmekaThumbnailUrl))?.ToArray() ?? []
                    }).ToArray()
                }).
                ToList();
        }
        return null;

    }

    public List<PlaceInterest>? GetInaccessiblePlaces(PlacesParameters parameters)
    {
        if (parameters.InaccessiblePlacesIds == null)
            return null;

        if (parameters.City?.Contains("prague") ?? false)
            return context.PraguePlacesOfInterestTimelines.
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

        return null;

    }

    public List<AddressWithVictims>? GetAddressesWithVictims(PlacesParameters parameters)
    {
        if (parameters.AddressesIds == null)
            return null;

        if (parameters.City?.Contains("prague") ?? false)
        {
            return context.PragueAddressesStatsTimelines.Where(p => parameters.AddressesIds.Contains(p.Id)).
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
                    PragueAddress = a.ConvertToPragueAddressesStatsTimelineShared(),
                    Victims = context.PragueVictimsTimelines.Where(b => b.PlaceId == a.Id).OrderBy(a => a.Label).
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
        else if (parameters.City.IsMemoMapCity())
        {
            using var context = factory.GetContext(parameters.City);
            return [.. context.Places.
                Include(a => a.EntitiesXPlaces).ThenInclude(a => a.Entity).ThenInclude(a => a.EntitiesXMedia).ThenInclude(a => a.Media).
                Include(a => a.EntitiesXPlaces).ThenInclude(a => a.RelationshipTypeNavigation).
                Include(a => a.EntitiesXPlaces).ThenInclude(a => a.Entity).ThenInclude(a => a.EntitiesXNarrativeMaps).
                Where(p => parameters.AddressesIds.Contains(p.Id)).
                AsEnumerable().
                Select(a => new AddressWithVictims
                {
                    Address = new AddressInfo
                    {
                        Cs = a.LabelCs,
                        En = a.LabelEn,
                    },
                    Victims = [.. a.EntitiesXPlaces.Select(b => new VictimShortInfoModel
                    {
                        Id = b?.Entity.Id ?? 0,
                        LongInfo = true,
                        Photo = b?.Entity.EntitiesXMedia?.Select(c => c.Media)?.FirstOrDefault()?.OmekaUrl,
                        Label = b?.Entity.Surname + ", " + b?.Entity.Firstname + (b?.Entity.Birthdate != null ? " (*" + b?.Entity.Birthdate?.ToString("d.M.yyyy") + ")" : ""),
                        RelationshipToAddressType = b?.RelationshipType,
                        NarrativeMapId = b?.Entity.EntitiesXNarrativeMaps.FirstOrDefault()?.NarrativeMapId,
                    })]
                })];
        }
        return null;

    }

    public List<AddressWithVictims>? GetAddressesLastResidenceWithVictims(PlacesParameters parameters)
    {
        if (parameters.AddressesLastResidenceIds == null)
            return null;

        if (parameters.City != "prague_last_residence")
            return null;

        return [.. context.PragueAddressesStatsTimelines.Where(p => parameters.AddressesLastResidenceIds.Contains(p.Id)).
            AsEnumerable().
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
                PragueAddress = a.ConvertToPragueAddressesStatsTimelineShared(),
                Victims = context.PragueVictimsTimelines.Where(b => b.PragueLastResidences.Any(c => c.AddressId == a.Id)).OrderBy(a => a.Label).
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
            })];

    }

    private static Polygon GetBBox(PointModel southWestPoint, PointModel northEastPoint)
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

        var result = new VictimLongInfoModel();

        if (city.IsMemoMapCity())
        {
            using var context = factory.GetContext(city);
            result = context.Entities.
                Include(a => a.FateNavigation).
                Include(a => a.EntitiesXNarrativeMaps).
                Include(a => a.EntitiesXTransports).ThenInclude(a => a.Transport).ThenInclude(a => a.PlaceFromNavigation).
                Include(a => a.EntitiesXTransports).ThenInclude(a => a.Transport).ThenInclude(a => a.PlaceToNavigation).
                Include(a => a.EntitiesXMedia).ThenInclude(a => a.Media).
                Include(a => a.EntitiesXPlaces).ThenInclude(a => a.Place).
                Include(a => a.EntitiesXPlaces).ThenInclude(a => a.RelationshipTypeNavigation).
                Include(a => a.EntitiesXEntityEntity2s).ThenInclude(a => a.Entity1).ThenInclude(a => a.EntitiesXMedia).ThenInclude(a => a.Media).
                Include(a => a.EntitiesXEntityEntity2s).ThenInclude(a => a.Entity1).ThenInclude(a => a.EntitiesXNarrativeMaps).
                Include(a => a.EntitiesXEntityEntity2s).ThenInclude(a => a.RelationshipTypeNavigation).
                Include(a => a.DocumentsXEntities).ThenInclude(a => a.Document).ThenInclude(a => a.DocumentsXMedia).ThenInclude(a => a.Media).
                Include(a => a.DocumentsXEntities).ThenInclude(a => a.Document).ThenInclude(a => a.CreationPlaceNavigation).
                Where(a => a.Id == id).
                AsEnumerable().
                Select(b => new VictimLongInfoModel
                {
                    Id = b.Id,
                    NarrativeMapId = b.EntitiesXNarrativeMaps.FirstOrDefault()?.NarrativeMapId,
                    Maidenname = b.Maidenname,
                    Title = b.Title,
                    BirthDate = b.Birthdate,
                    DeathDate = b.Deathdate,
                    DeathDateText = b.DeathdateText,
                    BirthDateText = b.BirthdateText,
                    DescriptionCs = b?.DescriptionCs,
                    DescriptionEn = b?.DescriptionEn,
                    Label = b?.Surname + ", " + b?.Firstname + (b?.Birthdate != null ? " (*" + b?.Birthdate?.ToString("d.M.yyyy") + ")" : ""),
                    FateCs = b?.Sex == 3 ? b?.FateNavigation?.LabelCs?.Replace("/", "") : b?.FateNavigation?.LabelCs?.Replace("/a", ""),
                    FateEn = b?.FateNavigation?.LabelEn,
                    Photo = b?.EntitiesXMedia.Select(a => a.Media).FirstOrDefault()?.OmekaUrl,
                    Places = b?.EntitiesXPlaces.OrderBy(a => a.Id).Select(a => new AddressInfo
                    {
                        Id = a.Place.Id,
                        Cs = a.Place.LabelCs,
                        En = a.Place.LabelEn ?? a.Place.LabelCs,
                        Type = a.RelationshipType,
                        TypeCs = a.RelationshipTypeNavigation.LabelCs,
                        TypeEn = a.RelationshipTypeNavigation.LabelEn,
                        DateFrom = a.DateFrom,
                        DateTo = a.DateTo,
                    }).ToArray(),
                    Documents = b?.DocumentsXEntities.OrderBy(a => a.Id).Select(c => c.Document).Select(c => new Shared.Document
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
                        Url = c?.DocumentsXMedia?.Select(d => new OmekaUrl(d?.Media?.OmekaUrl, d?.Media?.OmekaThumbnailUrl))?.ToArray() ?? []
                    }).ToArray(),
                    RelatedPersons = b?.EntitiesXEntityEntity2s.Select(a => new VictimShortInfoModel
                    {
                        Id = a.Entity1.Id,
                        Name = a.Entity1.Surname + ", " + a.Entity1.Firstname,
                        Birthdate = a.Entity1.Birthdate?.ToString("d.M.yyyy"),
                        Photo = a.Entity1.EntitiesXMedia.Select(c => c.Media).FirstOrDefault()?.OmekaUrl,
                        RelationshipToPersonCs = a.RelationshipTypeNavigation.LabelCs,
                        RelationshipToPersonEn = a.RelationshipTypeNavigation.LabelEn,
                        RelationshipToPersonType = a.RelationshipType,
                        LongInfo = true,
                        NarrativeMapId = a?.Entity1.EntitiesXNarrativeMaps.FirstOrDefault()?.NarrativeMapId,
                    }).ToArray(),
                    Transports = b?.EntitiesXTransports.OrderBy(a => a.Id).Select(a => new Shared.Transport
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
        }
        return result;
    }

    public List<Shared.NarrativeMap> GetAllNarrativeMaps(string city)
    {
        var result = new List<Shared.NarrativeMap>();
        if (city.IsMemoMapCity())
        {
            using var MemoMapContext = factory.GetContext(city);
            result = MemoMapContext.NarrativeMaps.
                Select(a => new Shared.NarrativeMap
                {
                    Id = a.Id,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn
                }).ToList();

            var mainPoints = MemoMapContext.NarrativeMapStopsXPlaces.
                Include(a=> a.Place).
                Include(a => a.NarrativeMapStop).ThenInclude(a => a.NarrativeMapsXNarrativeMapStops).
                Where(a => a.RelationshipTypeNavigation.LabelEn == "main point").
                AsEnumerable().
                Select(a => new
                {
                    a.NarrativeMapStop.NarrativeMapsXNarrativeMapStops.FirstOrDefault()?.NarrativeMapId,
                    a.NarrativeMapStopId,
                    a.NarrativeMapStop.LabelCs,
                    a.NarrativeMapStop.LabelEn,
                    a.Place
                }).ToArray();

            foreach (var map in result)
            {
                map.MainPoints = mainPoints.
                    Where(a => a.NarrativeMapId == map.Id).
                    Select(a => new Shared.Place
                    {
                        Id = a.Place.Id,
                        LabelCs = map.LabelCs + "<br/>" + a.LabelCs,
                        LabelEn = map.LabelEn + "<br/>" + a.LabelEn,
                        MapPoint = a.Place.Geography.AsJson(),
                        NarrativeMapId = a.NarrativeMapId,
                        StopId = a.NarrativeMapStopId,
                        Type = "main point"
                    }).ToArray();
            }

        }
        return result;
    }
    public Shared.NarrativeMap? GetNarrativeMap(long id, string city)
    {
        var map = new Shared.NarrativeMap();
        if (!city.IsMemoMapCity())
            return null;

        using var MemoMapContext = factory.GetContext(city);
        map = MemoMapContext.NarrativeMaps.
            Include(a => a.EntitiesXNarrativeMaps).ThenInclude(a => a.Entity).ThenInclude(a => a.EntitiesXMedia).ThenInclude(a => a.Media).
            Include(a => a.NarrativeMapsXNarrativeMapStops).
            Select(a => new Shared.NarrativeMap
            {
                Id = a.Id,
                LabelCs = a.LabelCs,
                LabelEn = a.LabelEn,
                DescriptionCs = a.DescriptionCs,
                DescriptionEn = a.DescriptionEn,
                Type = a.Type
            }).
            FirstOrDefault(a => a.Id == id);

        var stops = Array.Empty<Shared.NarrativeMapStop>();

        stops = MemoMapContext.NarrativeMapStops.
            Include(a => a.NarrativeMapsXNarrativeMapStops).
            Include(a => a.NarrativeMapStopsXPlaces).ThenInclude(a => a.Place).
            Include(a => a.NarrativeMapStopsXPlaces).ThenInclude(a => a.RelationshipTypeNavigation).
            Include(a => a.DocumentsXNarrativeMapStops).ThenInclude(a => a.Document).ThenInclude(a => a.DocumentsXMedia).ThenInclude(a => a.Media).
            Where(a => a.NarrativeMapsXNarrativeMapStops.Any(b => b.NarrativeMapId == id)).
            AsEnumerable().
            Select(a =>
            {
                StringHelpers.ParseDescriptionSections(a.DescriptionCs, out var descriptionCs, out var descriptionSectionsCs);
                StringHelpers.ParseDescriptionSections(a.DescriptionEn, out var descriptionEn, out var descriptionSectionsEn);
                return new Shared.NarrativeMapStop
                {
                    Id = a.Id,
                    LabelCs = a.LabelCs,
                    LabelEn = a.LabelEn,
                    DateCs = a.DateCs,
                    DateEn = a.DateEn,
                    DescriptionCs = descriptionCs,
                    DescriptionEn = descriptionEn,
                    DescriptionSectionsCs = descriptionSectionsCs,
                    DescriptionSectionsEn = descriptionSectionsEn,
                    Places = a.NarrativeMapStopsXPlaces.Select(b => new Shared.Place
                    {
                        Id = b.Id,
                        NarrativeMapId = a.NarrativeMapsXNarrativeMapStops.FirstOrDefault()?.NarrativeMapId ?? 0,
                        LabelCs = b.RelationshipTypeNavigation.LabelEn == "main point" ? (a.LabelCs == b.Place.LabelCs ? a.LabelCs : a.LabelCs + "<br/>" + b.Place.LabelCs) : b.Place.LabelCs,
                        LabelEn = b.RelationshipTypeNavigation.LabelEn == "main point" ? (a.LabelEn == b.Place.LabelEn ? a.LabelEn : a.LabelEn + "<br/>" + b.Place.LabelEn) : b.Place.LabelEn,
                        AddressCs = b.Place.LabelCs,
                        AddressEn = b.Place.LabelEn,
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
                    Documents = a.DocumentsXNarrativeMapStops.Select(b => b.Document).Select(b => new Shared.Document
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
                        Url = b?.DocumentsXMedia?.Select(d => new OmekaUrl(d?.Media?.OmekaUrl, d?.Media?.OmekaThumbnailUrl))?.ToArray() ?? []
                    }).ToArray()
                };
            }).ToArray();

        map.Stops = stops;

        return map;
    }

    public async Task<List<Shared.MapObject>> GetWFSObjects(WFSParameters parameters, HttpClient httpClient)
    {
        var result = new List<Shared.MapObject>();

        if (parameters.WFSLayers == null || !parameters.WFSLayers.Any())
            return result;

        foreach (var layer in parameters.WFSLayers)
        {
            if (string.IsNullOrWhiteSpace(layer.Url))
                continue;

            try
            {
                var jsonString = await httpClient.GetStringAsync(layer.Url);

                // Parse the GeoJSON as a dynamic object first
                var geoJson = JsonConvert.DeserializeObject<dynamic>(jsonString);

                if (geoJson?.features != null)
                {
                    foreach (var feature in geoJson.features)
                    {
                        if (feature.geometry == null)
                            continue;

                        // Parse the geometry using GeoJsonSerializer
                        var geometryJson = JsonConvert.SerializeObject(feature.geometry);
                        Geometry? geometry = null;

                        using (var stringReader = new StringReader(geometryJson))
                        using (var jsonReader = new Newtonsoft.Json.JsonTextReader(stringReader))
                        {
                            var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
                            geometry = serializer.Deserialize<Geometry>(jsonReader);
                        }

                        var mapObject = new Shared.MapObject
                        {
                            PlaceType = layer.PlaceType,
                            LayerName = layer.Name,
                            GeographyMapPoint = geometry is Point ? geometry : null,
                            GeographyMapPolygon = geometry is Polygon or MultiPolygon ? geometry : null,
                            MapPoint = geometry is Point ? geometry.AsJson() : null,
                            MapPolygon = geometry is Polygon or MultiPolygon ? geometry.AsJson() : null,
                            LabelCs = feature.properties?.name?.ToString() ?? string.Empty,
                            LabelEn = feature.properties?.name?.ToString() ?? string.Empty
                        };

                        result.Add(mapObject);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle WFS request errors
                Console.WriteLine($"Error fetching WFS data from {layer.Url}: {ex.Message}");
            }
        }

        return result;
    }
}