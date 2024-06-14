using EhriMemoMap.Shared;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace EhriMemoMap.API.Services
{
    public partial class SolrService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _solrUrl;


        public SolrService(
            IConfiguration configuration,
            IHttpClientFactory clientFactory)
        {
            _solrUrl = configuration.GetSection("App")["SolrUrl"] ?? "";
            _clientFactory = clientFactory;
        }

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

        public async Task<List<Place>> SolrExecuteDocument(SolrQueryParameters queryParameters)
        {
            var result = new List<Place>();
            var client = _clientFactory.CreateClient();

            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("defType", "edismax"),
                new KeyValuePair<string, string>("indent", "true" ),
                new KeyValuePair<string, string>("q.op", "OR" ),

                new KeyValuePair<string, string>("fl", "label_cs, label_en, place_cs, place_en, place_de, place_current_cs, place_current_en, place_current_de, map_location, map_object, type, place_date"),
                new KeyValuePair<string, string>("q", GetNormalizedQuery(queryParameters.Query)),
                new KeyValuePair<string, string>("qf", "all"),
                new KeyValuePair<string, string>("wt", "json"),
                new KeyValuePair<string, string>("stopwords", "true"),
                new KeyValuePair<string, string>("rows", !string.IsNullOrEmpty(queryParameters.Query) && queryParameters.Query.Length > 3 ? "1000" : "0")
            };

            var url = _solrUrl + "select?" + parameters.Select(a => a.Key + "=" + a.Value).Aggregate((x, y) => x + "&" + y);
            var solrResult = await client.GetStringAsync(url);

            if (string.IsNullOrEmpty(solrResult))
                return new List<Place>();

            var solrResultObject = JObject.Parse(solrResult);

            foreach (var item in solrResultObject["response"]["docs"])
            {
                var itemType = item["type"]?.ToString();
                var newPlace = new Place
                {
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
    }
}
