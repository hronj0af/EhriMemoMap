using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using EhriMemoMap.Shared;

namespace EhriMemoMap.API.Helpers
{
    public static class StringHelpers
    {

        public static bool IsMemoMapCity(this string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return false;
            var lowerCity = city.Trim().ToLowerInvariant();
            return lowerCity switch
            {
                "ricany" or "pacov" => true,
                _ => false
            };
        }

        public static string GetCityName(this string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return "Unknown";
            var lowerCity = city.Trim().ToLowerInvariant();
            return lowerCity switch
            {
                "ricany" => "Říčany",
                "pacov" => "Pacov",
                "praha" => "Praha",
                _ => "Unknown"
            };

        }

        public static string? AsJson(this Geometry? geometry)
        {
            if (geometry == null)
                return null;

            // Převod geometrie na GeoJSON string
            var geoJsonWriter = new GeoJsonWriter();
            return geoJsonWriter.Write(geometry);
        }

        /// <summary>
        /// Parses description text with markdown-style ## headers into base description and sections
        /// </summary>
        /// <param name="description">The full description text with ## headers</param>
        /// <param name="baseDescription">Output: The text before the first ## header</param>
        /// <param name="sections">Output: Array of DescriptionSection objects</param>
        public static void ParseDescriptionSections(string? description, out string? baseDescription, out DescriptionSection[]? sections)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                baseDescription = description;
                sections = null;
                return;
            }

            // Split by ## headers
            var parts = Regex.Split(description, @"(?=##)");
            var sectionList = new List<DescriptionSection>();
            
            // First part (before any ##) is the base description
            baseDescription = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(baseDescription))
            {
                baseDescription = null;
            }

            // Process remaining parts as sections
            for (int i = 1; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                // Extract title (everything after ## until end of line)
                var match = Regex.Match(part, @"^##\s*(.+?)(?:\r?\n|$)");
                if (match.Success)
                {
                    var title = match.Groups[1].Value.Trim();
                    var text = part.Substring(match.Length).Trim();
                    
                    sectionList.Add(new DescriptionSection
                    {
                        Title = title,
                        Text = string.IsNullOrWhiteSpace(text) ? null : text
                    });
                }
            }

            sections = sectionList.Count > 0 ? sectionList.ToArray() : null;
        }
    }
}
