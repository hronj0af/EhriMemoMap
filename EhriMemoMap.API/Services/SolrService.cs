using EhriMemoMap.Shared;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace EhriMemoMap.API.Services
{

    public partial class SolrService(IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        private readonly string _solrUrl = configuration.GetSection("App")["SolrUrl"] ?? "";

        public string GetNormalizedQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
                return "*";

            return query.
                Split(" ").
                Select(a => a == "náměstí" || a == "nám." 
                                ? "(náměstí OR nám.)" 
                                : a).
                Aggregate((x, y) => x + " AND " + y);
        }

        public async Task<List<SolrPlace>> SolrExecuteDocument(SolrQueryParameters queryParameters)
        {
            var result = new List<SolrPlace>();
            var client = clientFactory.CreateClient();

            var parameters = new List<KeyValuePair<string, string>>
            {
                new("defType", "edismax"),
                new("indent", "true" ),
                new("q.op", "OR" ),
                new("fq", @$"city:""{queryParameters.City}"""),
                new("fl", "id label_cs, label_en, place_cs, place_en, place_de, place_current_cs, place_current_en, place_current_de, map_location, map_object, type, place_date"),
                new("q", GetNormalizedQuery(queryParameters.Query)),
                new("qf", "all"),
                new("wt", "json"),
                new("stopwords", "true"),
                new("rows", !string.IsNullOrEmpty(queryParameters.Query) && queryParameters.Query.Length > 3 ? "1000" : "0")
            };

            var url = _solrUrl + "select?" + parameters.Select(a => a.Key + "=" + a.Value).Aggregate((x, y) => x + "&" + y);
            var solrResult = await client.GetStringAsync(url);

            if (string.IsNullOrEmpty(solrResult))
                return new List<SolrPlace>();

            var solrResultObject = JObject.Parse(solrResult);

            foreach (var item in solrResultObject["response"]["docs"])
            {
                var itemType = item["type"]?.ToString();
                var newPlace = new SolrPlace
                {
                    Id = item["id"]?.ToString() ?? "",
                    Type = itemType?[0].ToString().ToUpper() + itemType?[1..],
                    LabelCs = item["label_cs"]?.ToString(),
                    LabelEn = item["label_en"]?.ToString(),
                    PlaceCs = item["place_cs"]?.ToString(),
                    PlaceEn = item["place_en"]?.ToString(),
                    PlaceDe = item["place_de"]?.ToString(),
                    PlaceCurrentCs = item["place_current_cs"]?.ToString(),
                    PlaceCurrentEn = item["place_current_en"]?.ToString(),
                    PlaceCurrentDe = item["place_current_de"]?.ToString(),
                    MapLocation = item["map_location"]?.ToString(),
                    MapObject = item["map_object"]?.ToString(),
                };

                if (!string.IsNullOrEmpty(item["place_date"]?[0].ToString()) && DateTime.TryParse(item["place_date"]?[0].ToString(), out DateTime newPlaceDate))
                {
                    newPlace.Date = newPlaceDate;
                }


                result.Add(newPlace);

            }

            return result;
        }

        /// <summary>
        /// Deletes all documents from the Solr core.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task DeleteAllDocumentsAsync()
        {
            string deleteUrl = $"{_solrUrl}update?commit=true";
            string jsonPayload = "{\"delete\": {\"query\":\"*:*\"}}";

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
                Console.WriteLine($"Error deleting documents from Solr: {ex.Message}");
                throw;
            }
        }
    }
}
