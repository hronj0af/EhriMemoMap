using EhriMemoMap.Helpers;
using System.Xml.Linq;

namespace EhriMemoMap.Models
{
    public class WMSResponse
    {
        public List<WMSLayer> Layers { get; set; }
        public WMSResponse(string xmlResponse) 
        {
            XDocument xdoc = XDocument.Parse(xmlResponse);
            Layers = xdoc.Descendants("Layer").Select(a => new WMSLayer(a)).ToList();
            Layers.RemoveAll(a => a.Features == null || a.Features.Count == 0);
        }
    }
}
