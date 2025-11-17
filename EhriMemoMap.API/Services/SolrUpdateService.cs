using EhriMemoMap.Data;
using EhriMemoMap.Data.Ricany;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Text;

namespace EhriMemoMap.API.Services
{

    public partial class SolrUpdateService(MemogisContext context, RicanyContext ricanyContext, IHttpClientFactory clientFactory, IConfiguration configuration)
    {
        private readonly string _solrUrl = configuration.GetSection("App")["SolrUrl"] ?? "";
        private readonly MemogisContext _context = context;
        private readonly RicanyContext _ricanyContext = ricanyContext;
        private readonly GeoJsonWriter _geoJsonWriter = new();

        public List<SolrPlaceForUpdate> GetPraguePlaces()
        {
            var places = new List<SolrPlaceForUpdate>();

            // Prague places of memory
            places.AddRange(_context.PraguePlacesOfMemories.AsEnumerable().Select(p => new SolrPlaceForUpdate
            {
                City = "prague",
                Id = $"{p.Id}.memory.prague",
                LabelCs = p.Label,
                LabelEn = p.Label,
                PlaceCs = p.AddressCs,
                PlaceEn = p.AddressEn,
                PlaceDe = "",
                PlaceCurrentCs = "",
                PlaceCurrentEn = "",
                PlaceCurrentDe = "",
                All = $"{p.Label ?? ""} {p.AddressCs ?? ""} {p.AddressEn ?? ""}",
                MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                MapObject = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                PlaceDate = null,
                Type = "memory"
            }));

            // Prague places of interest
            places.AddRange(_context.PraguePlacesOfInterests.AsEnumerable().Select(p => new SolrPlaceForUpdate
            {
                City = "prague",
                Id = $"{p.Id}.interest.prague",
                LabelCs = p.LabelCs,
                LabelEn = p.LabelEn,
                PlaceCs = p.AddressCs,
                PlaceEn = p.AddressEn,
                PlaceDe = "",
                PlaceCurrentCs = "",
                PlaceCurrentEn = "",
                PlaceCurrentDe = "",
                All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {p.AddressCs ?? ""} {p.AddressEn ?? ""}",
                MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                MapObject = p.GeographyPolygon != null ? _geoJsonWriter.Write(p.GeographyPolygon.Copy()) : null,
                PlaceDate = null,
                Type = p.Inaccessible == true ? "inaccessible" : "interest"
            }));

            // Prague incidents
            places.AddRange(_context.PragueIncidentsTimelines.AsEnumerable().Select(p => new SolrPlaceForUpdate
            {
                City = "prague",
                Id = $"{p.Id}.incident.prague",
                LabelCs = p.LabelCs,
                LabelEn = p.LabelEn,
                PlaceCs = p.PlaceCs,
                PlaceEn = p.PlaceEn,
                PlaceDe = "",
                PlaceCurrentCs = "",
                PlaceCurrentEn = "",
                PlaceCurrentDe = "",
                All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {p.PlaceCs ?? ""} {p.PlaceEn ?? ""}",
                MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                MapObject = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                PlaceDate = p.Date != null ? new List<string> { p.Date } : null,
                Type = "incident"
            }));

            // Prague victims - join with addresses
            var victims = from v in _context.PragueVictimsTimelines
                          join a in _context.PragueAddressesStatsTimelines on v.PlaceId equals a.Id
                          select new { Victim = v, Address = a };

            places.AddRange(victims.AsEnumerable().Select(va => new SolrPlaceForUpdate
            {
                City = "prague",
                Id = $"{va.Victim.Id}.victim.prague",
                LabelCs = va.Victim.Label,
                LabelEn = va.Victim.Label,
                PlaceCs = va.Address.AddressCs,
                PlaceEn = va.Address.AddressEn,
                PlaceDe = va.Address.AddressDe,
                PlaceCurrentCs = va.Address.AddressCurrentCs,
                PlaceCurrentEn = va.Address.AddressCurrentEn,
                PlaceCurrentDe = va.Address.AddressCurrentDe,
                All = $"{va.Victim.Label ?? ""} {va.Address.AddressCs ?? ""} {va.Address.AddressEn ?? ""} {va.Address.AddressDe ?? ""} {va.Address.AddressCurrentCs ?? ""} {va.Address.AddressCurrentEn ?? ""} {va.Address.AddressCurrentDe ?? ""}",
                MapLocation = va.Address.Geography != null ? _geoJsonWriter.Write(va.Address.Geography.Copy()) : null,
                MapObject = va.Address.Geography != null ? _geoJsonWriter.Write(va.Address.Geography.Copy()) : null,
                PlaceDate = null,
                Type = "address"
            }));

            return places;
        }

        public List<SolrPlaceForUpdate> GetPragueLastResidencePlaces()
        {
            var places = new List<SolrPlaceForUpdate>();

            // Prague places of memory (same as prague)
            places.AddRange(_context.PraguePlacesOfMemories.AsEnumerable().Select(p => new SolrPlaceForUpdate
            {
                City = "prague_last_residence",
                Id = $"{p.Id}.memory.prague_last_residence",
                LabelCs = p.Label,
                LabelEn = p.Label,
                PlaceCs = p.AddressCs,
                PlaceEn = p.AddressEn,
                PlaceDe = "",
                PlaceCurrentCs = "",
                PlaceCurrentEn = "",
                PlaceCurrentDe = "",
                All = $"{p.Label ?? ""} {p.AddressCs ?? ""} {p.AddressEn ?? ""}",
                MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                MapObject = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                PlaceDate = null,
                Type = "memory"
            }));

            // Prague places of interest (same as prague)
            places.AddRange(_context.PraguePlacesOfInterests.AsEnumerable().Select(p => new SolrPlaceForUpdate
            {
                City = "prague_last_residence",
                Id = $"{p.Id}.interest.prague_last_residence",
                LabelCs = p.LabelCs,
                LabelEn = p.LabelEn,
                PlaceCs = p.AddressCs,
                PlaceEn = p.AddressEn,
                PlaceDe = "",
                PlaceCurrentCs = "",
                PlaceCurrentEn = "",
                PlaceCurrentDe = "",
                All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {p.AddressCs ?? ""} {p.AddressEn ?? ""}",
                MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                MapObject = p.GeographyPolygon != null ? _geoJsonWriter.Write(p.GeographyPolygon.Copy()) : null,
                PlaceDate = null,
                Type = p.Inaccessible == true ? "inaccessible" : "interest"
            }));

            // Prague incidents (same as prague)
            places.AddRange(_context.PragueIncidentsTimelines.AsEnumerable().Select(p => new SolrPlaceForUpdate
            {
                City = "prague_last_residence",
                Id = $"{p.Id}.incident.prague_last_residence",
                LabelCs = p.LabelCs,
                LabelEn = p.LabelEn,
                PlaceCs = p.PlaceCs,
                PlaceEn = p.PlaceEn,
                PlaceDe = "",
                PlaceCurrentCs = "",
                PlaceCurrentEn = "",
                PlaceCurrentDe = "",
                All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {p.PlaceCs ?? ""} {p.PlaceEn ?? ""}",
                MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                MapObject = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                PlaceDate = p.Date != null ? new List<string> { p.Date } : null,
                Type = "incident"
            }));

            // Prague victims (same as prague) - join with addresses
            var victims = _context.PragueVictimsTimelines.Join(_context.PragueAddressesStatsTimelines,
                    v => v.PlaceId,
                    a => a.Id,
                    (v, a) => new { Victim = v, Address = a });

            places.AddRange(victims.AsEnumerable().Select(va => new SolrPlaceForUpdate
            {
                City = "prague_last_residence",
                Id = $"{va.Victim.Id}.victim.prague_last_residence",
                LabelCs = va.Victim.Label,
                LabelEn = va.Victim.Label,
                PlaceCs = va.Address.AddressCs,
                PlaceEn = va.Address.AddressEn,
                PlaceDe = va.Address.AddressDe,
                PlaceCurrentCs = va.Address.AddressCurrentCs,
                PlaceCurrentEn = va.Address.AddressCurrentEn,
                PlaceCurrentDe = va.Address.AddressCurrentDe,
                All = $"{va.Victim.Label ?? ""} {va.Address.AddressCs ?? ""} {va.Address.AddressEn ?? ""} {va.Address.AddressDe ?? ""} {va.Address.AddressCurrentCs ?? ""} {va.Address.AddressCurrentEn ?? ""} {va.Address.AddressCurrentDe ?? ""}",
                MapLocation = va.Address.Geography != null ? _geoJsonWriter.Write(va.Address.Geography.Copy()) : null,
                MapObject = va.Address.Geography != null ? _geoJsonWriter.Write(va.Address.Geography.Copy()) : null,
                PlaceDate = null,
                Type = "address"
            }));

            // Prague last residence victims
            places.AddRange(_context.PragueLastResidences
                .Include(lr => lr.Victim)
                .Include(lr => lr.Address)
                .AsEnumerable()
                .Select(lr => new SolrPlaceForUpdate
                {
                    City = "prague_last_residence",
                    Id = $"{lr.Id}.victim_last_residence.prague_last_residence",
                    LabelCs = lr.Victim.Label,
                    LabelEn = lr.Victim.Label,
                    PlaceCs = lr.Address.AddressCs,
                    PlaceEn = lr.Address.AddressEn,
                    PlaceDe = lr.Address.AddressDe,
                    PlaceCurrentCs = lr.Address.AddressCurrentCs,
                    PlaceCurrentEn = lr.Address.AddressCurrentEn,
                    PlaceCurrentDe = lr.Address.AddressCurrentDe,
                    All = $"{lr.Victim.Label ?? ""} {lr.Address.AddressCs ?? ""} {lr.Address.AddressEn ?? ""} {lr.Address.AddressDe ?? ""} {lr.Address.AddressCurrentCs ?? ""} {lr.Address.AddressCurrentEn ?? ""} {lr.Address.AddressCurrentDe ?? ""}",
                    MapLocation = lr.Address.Geography != null ? _geoJsonWriter.Write(lr.Address.Geography.Copy()) : null,
                    MapObject = lr.Address.Geography != null ? _geoJsonWriter.Write(lr.Address.Geography.Copy()) : null,
                    PlaceDate = null,
                    Type = "address"
                }));

            return places;
        }

        public List<SolrPlaceForUpdate> GetPacovPlaces()
        {
            var places = new List<SolrPlaceForUpdate>();

            // Pacov POIs
            places.AddRange(_context.PacovPois
                .Include(p => p.Place)
                .AsEnumerable()
                .Select(p => new SolrPlaceForUpdate
                {
                    City = "pacov",
                    Id = $"{p.Id}.interest.pacov",
                    LabelCs = p.LabelCs,
                    LabelEn = p.LabelEn,
                    PlaceCs = p.Place?.LabelCs,
                    PlaceEn = p.Place?.LabelEn,
                    PlaceDe = "",
                    PlaceCurrentCs = "",
                    PlaceCurrentEn = "",
                    PlaceCurrentDe = "",
                    All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {p.Place?.LabelCs ?? ""} {p.Place?.LabelEn ?? ""}",
                    MapLocation = p.Place?.Geography != null ? _geoJsonWriter.Write(p.Place.Geography.Copy()) : null,
                    MapObject = null,
                    PlaceDate = null,
                    Type = "interest"
                }));

            // Pacov entities/victims at places
            places.AddRange(_context.PacovPlaces
                .Include(p => p.PacovEntitiesXPlaces)
                    .ThenInclude(ep => ep.Entity)
                .Where(p => p.Type == "house" && p.TownCs == "Pacov")
                .AsEnumerable()
                .SelectMany(p => p.PacovEntitiesXPlaces.Select(ep => new SolrPlaceForUpdate
                {
                    City = "pacov",
                    Id = $"{p.Id}.address_{ep.EntityId}.victim.pacov",
                    LabelCs = $"{ep.Entity.Surname}, {ep.Entity.Firstname} (* {ep.Entity.Birthdate?.ToString("d.M.yyyy")})",
                    LabelEn = $"{ep.Entity.Surname}, {ep.Entity.Firstname} (* {ep.Entity.Birthdate?.ToString("d.M.yyyy")})",
                    PlaceCs = p.LabelCs,
                    PlaceEn = p.LabelEn,
                    PlaceDe = "",
                    PlaceCurrentCs = "",
                    PlaceCurrentEn = "",
                    PlaceCurrentDe = "",
                    All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {ep.Entity.Firstname ?? ""} {ep.Entity.Surname ?? ""}",
                    MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                    MapObject = null,
                    PlaceDate = null,
                    Type = "address"
                })));

            return places;
        }

        public List<SolrPlaceForUpdate> GetRicanyPlaces()
        {
            var places = new List<SolrPlaceForUpdate>();

            // Ricany places of memory (stolpersteine)
            var ricanyMemories = _ricanyContext.PlacesOfMemories.
                Where(a=>a.Type != "stolperstein").
                Include(px => px.PlacesXPlacesOfMemories).ThenInclude(a => a.Place).AsEnumerable();

            foreach (var memory in ricanyMemories)
            {
                var place = memory.PlacesXPlacesOfMemories.FirstOrDefault()?.Place;

                places.Add(new SolrPlaceForUpdate
                {
                    City = "ricany",
                    Id = $"{memory.Id}.memory.ricany",
                    LabelCs = memory.LabelCs,
                    LabelEn = memory.LabelEn,
                    PlaceCs = place?.LabelCs,
                    PlaceEn = place?.LabelEn,
                    PlaceDe = "",
                    PlaceCurrentCs = "",
                    PlaceCurrentEn = "",
                    PlaceCurrentDe = "",
                    All = $"{memory.LabelCs ?? ""} {place?.LabelCs ?? ""}",
                    MapLocation = place?.Geography != null ? _geoJsonWriter.Write(place.Geography.Copy()) : null,
                    MapObject = place?.Geography != null ? _geoJsonWriter.Write(place.Geography.Copy()) : null,
                    PlaceDate = null,
                    Type = "memory"
                });
            }

            // Ricany POIs
            places.AddRange(_ricanyContext.Pois
                .Include(p => p.Place)
                .AsEnumerable()
                .Select(p => new SolrPlaceForUpdate
                {
                    City = "ricany",
                    Id = $"{p.Id}.interest.ricany",
                    LabelCs = p.LabelCs,
                    LabelEn = p.LabelEn,
                    PlaceCs = p.Place?.LabelCs,
                    PlaceEn = p.Place?.LabelEn,
                    PlaceDe = "",
                    PlaceCurrentCs = "",
                    PlaceCurrentEn = "",
                    PlaceCurrentDe = "",
                    All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {p.Place?.LabelCs ?? ""} {p.Place?.LabelEn ?? ""}",
                    MapLocation = p.Place?.Geography != null ? _geoJsonWriter.Write(p.Place.Geography.Copy()) : null,
                    MapObject = null,
                    PlaceDate = null,
                    Type = "interest"
                }));

            // Ricany entities/victims at places
            places.AddRange(_ricanyContext.Places
                .Include(p => p.EntitiesXPlaces)
                    .ThenInclude(ep => ep.Entity)
                .Where(p => p.Type == "house" && p.TownCs == "Říčany")
                .AsEnumerable()
                .SelectMany(p => p.EntitiesXPlaces.Select(ep => new SolrPlaceForUpdate
                {
                    City = "ricany",
                    Id = $"{p.Id}.address_{ep.EntityId}.victim.ricany",
                    LabelCs = $"{ep.Entity.Surname}, {ep.Entity.Firstname} (* {ep.Entity.Birthdate?.ToString("d.M.yyyy")})",
                    LabelEn = $"{ep.Entity.Surname}, {ep.Entity.Firstname} (* {ep.Entity.Birthdate?.ToString("d.M.yyyy")})",
                    PlaceCs = p.LabelCs,
                    PlaceEn = p.LabelEn,
                    PlaceDe = "",
                    PlaceCurrentCs = "",
                    PlaceCurrentEn = "",
                    PlaceCurrentDe = "",
                    All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {ep.Entity.Firstname ?? ""} {ep.Entity.Surname ?? ""}",
                    MapLocation = p.Geography != null ? _geoJsonWriter.Write(p.Geography.Copy()) : null,
                    MapObject = null,
                    PlaceDate = null,
                    Type = "address"
                })));

            // Ricany memorials
            places.AddRange(_ricanyContext.Events
                .Include(p => p.EventsXPlaces)
                    .ThenInclude(ep => ep.Place)
                .AsEnumerable()
                .SelectMany(p => p.EventsXPlaces.Select(ep => new SolrPlaceForUpdate
                {
                    City = "ricany",
                    Id = $"{p.Id}.event_{ep.EventId}.ricany",
                    LabelCs = $"{p.LabelCs}",
                    LabelEn = $"{p.LabelEn}",
                    PlaceCs = ep.Place?.LabelCs,
                    PlaceEn = ep.Place?.LabelCs,
                    PlaceDe = "",
                    PlaceCurrentCs = "",
                    PlaceCurrentEn = "",
                    PlaceCurrentDe = "",
                    All = $"{p.LabelCs ?? ""} {p.LabelEn ?? ""} {ep.Place?.LabelCs ?? ""} {ep.Place?.LabelEn ?? ""}",
                    MapLocation = ep.Place?.Geography != null ? _geoJsonWriter.Write(ep.Place.Geography.Copy()) : null,
                    MapObject = null,
                    PlaceDate = null,
                    Type = "memorial"
                })));

            return places;
        }

        public async Task<int> UpdateAllPlacesAsync()
        {
            var allPlaces = new List<SolrPlaceForUpdate>();

            allPlaces.AddRange(GetPraguePlaces());
            allPlaces.AddRange(GetPragueLastResidencePlaces());
            allPlaces.AddRange(GetPacovPlaces());
            allPlaces.AddRange(GetRicanyPlaces());

            // Přidávání dokumentů do Solr v dávkách po 100
            const int batchSize = 1000;
            var totalCount = allPlaces.Count;

            for (int i = 0; i < totalCount; i += batchSize)
            {
                var batch = allPlaces.Skip(i).Take(batchSize).ToList();
                await AddOrUpdateDocumentsAsync(batch);
            }

            return totalCount;
        }

        /// <summary>
        /// Deletes documents from the Solr core by query.
        /// </summary>
        /// <param name="query">The query to match documents to delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task DeleteDocumentsByQueryAsync(string query)
        {
            string deleteUrl = $"{_solrUrl}update?commit=true";
            string jsonPayload = $"{{\"delete\": {{\"query\":\"{query}\"}}}}";

            try
            {
                var client = clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(jsonPayload, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                HttpResponseMessage response = await client.PostAsync(deleteUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting documents from Solr by query: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds or updates a list of SolrDocument to the Solr core.
        /// </summary>
        /// <param name="documents">The list of SolrDocument to add or update.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddOrUpdateDocumentsAsync(IEnumerable<SolrPlaceForUpdate> documents)
        {
            if (documents == null || !documents.Any())
                return;

            string solrUrl = $"{_solrUrl}update/json/docs?commit=true";

            string jsonPayload = JsonConvert.SerializeObject(documents, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        OverrideSpecifiedNames = false
                    }
                }
            });

            try
            {
                var client = clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(solrUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding/updating documents to Solr: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds or updates a single SolrDocument to the Solr core.
        /// </summary>
        /// <param name="place">The SolrDocument to add or update.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddOrUpdateDocumentAsync(SolrPlaceForUpdate place)
        {
            await AddOrUpdateDocumentsAsync(new[] { place });
        }

    }

    public class SolrPlaceForUpdate
    {
        [JsonProperty("all")]
        public string? All { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("label_cs")]
        public string? LabelCs { get; set; }

        [JsonProperty("label_en")]
        public string? LabelEn { get; set; }

        [JsonProperty("map_location")]
        public string? MapLocation { get; set; }

        [JsonProperty("map_object")]
        public string? MapObject { get; set; }

        [JsonProperty("place_cs")]
        public string? PlaceCs { get; set; }

        [JsonProperty("place_current_cs")]
        public string? PlaceCurrentCs { get; set; }

        [JsonProperty("place_current_de")]
        public string? PlaceCurrentDe { get; set; }

        [JsonProperty("place_current_en")]
        public string? PlaceCurrentEn { get; set; }

        [JsonProperty("place_date")]
        public List<string>? PlaceDate { get; set; }

        [JsonProperty("place_de")]
        public string? PlaceDe { get; set; }

        [JsonProperty("place_en")]
        public string? PlaceEn { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }


    }

}
