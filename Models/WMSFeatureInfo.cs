using System.Xml.Linq;

namespace EhriMemoMap.Models
{

    public class WMSFeatureInfo
    {
        public WMSFeatureInfo(XElement xdoc, WMSFeatureType type)
        {
            FeatureInfoType = type;
            foreach (var element in xdoc.Descendants("Attribute"))
            {
                switch (element.Attribute("name")?.Value)
                {
                    case "Jméno (datum narození)": 
                        NameAndBirthDate = element.Attribute("value")?.Value;
                        break;
                    case "Detaily": 
                        Details = element.Attribute("value")?.Value;
                        break;
                    case "Adresa": 
                        Address = element.Attribute("value")?.Value;
                        break;
                    case "Adresa (česky, z doby okupace)": 
                        AddressCzechOccupation = element.Attribute("value")?.Value;
                        break;
                    case "Adresa (německy, z doby okupace)": 
                        AddressGermanOccupation = element.Attribute("value")?.Value;
                        break;
                    case "Počet židovských obyvatel (1. 10. 1941)":
                        JewsCitizens1941 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Zavražděno": 
                        Murdered = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Přežilo": 
                        Survived = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Osud neznámý": 
                        FateUnknown = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1942": 
                        NotDeportedUntil_1_1_1942 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1943":
                        NotDeportedUntil_1_1_1943 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1944": 
                        NotDeportedUntil_1_1_1944 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1945": 
                        NotDeportedUntil_1_1_1945 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 9. 5. 1945": 
                        NotDeportedUntil_9_5_1945 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Název": 
                        Title = element.Attribute("value")?.Value;
                        break;
                    case "Popis": 
                        Description = element.Attribute("value")?.Value;
                        break;
                    case "Typ": 
                        Type = element.Attribute("value")?.Value;
                        break;
                    case "Specifikace": 
                        Specification = element.Attribute("value")?.Value;
                        break;
                    case "Typ (alternativní)": 
                        TypeAlternative = element.Attribute("value")?.Value;
                        break;
                    case "Specifikace (alternativní)": 
                        SpecificationAlternative = element.Attribute("value")?.Value;
                        break;
                    case "Datum": 
                        Date = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? DateTime.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Místo": 
                        Place = element.Attribute("value")?.Value;
                        break;
                    case "Dokumenty": 
                        Documents = element.Attribute("value")?.Value;
                        break;
                }
            }
        }


        public string? NameAndBirthDate { get; set; }
        public string? Details { get; set; }
        public string? Address { get; set; }
        public string? AddressCzechOccupation { get; set; }
        public string? AddressGermanOccupation { get; set; }
        public int? JewsCitizens1941 { get; set; }
        public int? Murdered { get; set; }
        public int? Survived { get; set; }
        public int? FateUnknown { get; set; }
        public int? NotDeportedUntil_1_1_1942 { get; set; }
        public int? NotDeportedUntil_1_1_1943 { get; set; }
        public int? NotDeportedUntil_1_1_1944 { get; set; }
        public int? NotDeportedUntil_1_1_1945 { get; set; }
        public int? NotDeportedUntil_9_5_1945 { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Specification { get; set; }
        public string? TypeAlternative { get; set; }
        public string? SpecificationAlternative { get; set; }
        public DateTime? Date { get; set; }
        public string? Place { get; set; }
        public string? Documents { get; set; }
        public WMSFeatureType? FeatureInfoType { get; set; }
    }

}
