using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EhriMemoMap.API.Helpers
{
    public static class StringHelpers
    {
        public static string? AsJson(this Geometry? geometry)
        {
            if (geometry == null)
                return null;

            // Převod geometrie na GeoJSON string
            var geoJsonWriter = new GeoJsonWriter();
            return geoJsonWriter.Write(geometry);
        }
    }
}
