using System.Xml.Linq;

namespace EhriMemoMap.Models
{
    public class WMSLayer
    {
        public WMSFeatureType Type { get; set; }
        public List<WMSFeatureInfo> Features { get; set; }
        public WMSLayer(XElement xdoc) 
        {
            Type = Enum.Parse<WMSFeatureType>(xdoc.Attribute("name").Value);
            Features = xdoc.Descendants("Feature").Select(a => new WMSFeatureInfo(a, Type)).ToList();
        }

    }
    
}
