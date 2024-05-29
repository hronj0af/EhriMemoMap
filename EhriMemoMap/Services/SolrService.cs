using EhriMemoMap.Models;
using EhriMemoMap.Shared;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
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

        public string GetNormalizedQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
                return "*";

            // jestlize je query adresa, tak se query obalí do uvozovek
            var splitQuery = query.Split(" ");
            if (splitQuery.Length > 1 && Regex.IsMatch(splitQuery[^1], @"^[0-9]+") )
                return "\"" + query + "\"";

            if (splitQuery.Length == 2)
                return $"(\"{query}\" OR \"{splitQuery[1]} {splitQuery[0]} \")";

            return query;

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
                new KeyValuePair<string, string>("q", GetNormalizedQuery(query)),
                new KeyValuePair<string, string>("qf", "label_cs label_en place_cs place_en"),
                new KeyValuePair<string, string>("wt", "json"),
                new KeyValuePair<string, string>("stopwords", "true"),
                new KeyValuePair<string, string>("rows", !string.IsNullOrEmpty(query) && query.Length > 3 ? "1000" : "0")
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
