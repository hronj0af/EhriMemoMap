using EhriMemoMap.Models;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EhriMemoMap.Services
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

        public async Task<List<Place>> SolrExecuteDocument(string query)
        {
            var result = new List<Place>();
            var client = _clientFactory.CreateClient();

            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("defType", "edismax"),
                new KeyValuePair<string, string>("indent", "true" ),
                new KeyValuePair<string, string>("q.op", "OR" ),

                new KeyValuePair<string, string>("fl", "label_cs, label_en, place_cs, place_en, map_location, map_object, type, place_date"),
                new KeyValuePair<string, string>("q", !string.IsNullOrEmpty(query) ? query + "* " + query: "*"),
                new KeyValuePair<string, string>("qf", "label_cs label_en place_cs place_en"),
                new KeyValuePair<string, string>("wt", "json"),
                new KeyValuePair<string, string>("stopwords", "true"),
                new KeyValuePair<string, string>("rows", !string.IsNullOrEmpty(query) && query.Length > 3 ? "100" : "0")
            };

            var url = _solrUrl + "select?" + parameters.Select(a => a.Key + "=" + a.Value).Aggregate((x, y) => x + "&" + y);
            var solrResult = await client.GetStringAsync(url);

            if (string.IsNullOrEmpty(solrResult))
                return new List<Place>();

            var solrResultObject = JObject.Parse(solrResult);

            foreach (var item in solrResultObject["response"]["docs"])
            {
                var newPlace = new Place
                {
                    Type = item["type"]?.ToString(),
                    LabelCs = item["label_cs"]?.ToString(),
                    LabelEn = item["label_en"]?.ToString(),
                    PlaceCs = item["place_cs"]?.ToString(),
                    PlaceEn = item["place_en"]?.ToString(),
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
