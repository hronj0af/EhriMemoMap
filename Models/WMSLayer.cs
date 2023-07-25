using System.Xml.Linq;

namespace EhriMemoMap.Models
{
    public class WMSLayer
    {
        public WMSFeatureType Type { get; set; }
        public List<WMSFeatureInfo> Features { get; set; }
        public WMSLayer(XElement xdoc) 
        {
            switch (xdoc.Attribute("name")?.Value)
            {
                case "Neprisupna_mista":
                case "Inaccessible_places":
                    Type = WMSFeatureType.Neprisupna_mista;
                    break;

                case "Obeti_holokaustu_z_Prahy":
                case "Holocaust_victims_from_Prague":
                    Type = WMSFeatureType.Obeti_holokaustu_z_Prahy;
                    break;

                case "Bydliste_Zidu_v_Praze_v_dobe_okupace":
                case "Jewish_homes_during_the_occupation":
                    Type = WMSFeatureType.Bydliste_Zidu_v_Praze_v_dobe_okupace;
                    break;

                case "Body_zajmu":
                case "Points_of_interest":
                    Type = WMSFeatureType.Body_zajmu;
                    break;

                case "Incidenty_v_prostoru_mesta_Prahy":
                case "Incidents_within_the_city_of_Prague":
                    Type = WMSFeatureType.Incidenty_v_prostoru_mesta_Prahy;
                    break;
            }

            Features = xdoc.Descendants("Feature").Select(a => new WMSFeatureInfo(a, Type)).ToList();
        }

    }
    
}
