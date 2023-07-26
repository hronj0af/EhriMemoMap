using System.Globalization;
using System.Text.RegularExpressions;
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
                    case "Victim":
                        NameAndBirthDate = element.Attribute("value")?.Value;
                        break;
                    case "Detaily":
                    case "Details":
                        Details = element.Attribute("value")?.Value;
                        break;
                    case "Adresa":
                    case "Address":
                        Address = element.Attribute("value")?.Value;
                        break;
                    case "Adresa (česky, z doby okupace)":
                    case "Address (Czech)":
                        AddressCzechOccupation = element.Attribute("value")?.Value;
                        break;
                    case "Adresa (německy, z doby okupace)":
                    case "Address (German)":
                        AddressGermanOccupation = element.Attribute("value")?.Value;
                        break;
                    case "Počet židovských obyvatel (1. 10. 1941)":
                    case "Jewish residents (October 1941)":
                        JewishResidents1941 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Zavražděno":
                    case "Murdered":
                        Murdered = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Přežilo":
                    case "Survived":
                        Survived = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Osud neznámý":
                    case "Fate unknown":
                        FateUnknown = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1942":
                    case "Present at 1942-01-01":
                        NotDeportedUntil_1_1_1942 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1943":
                    case "Present at 1943-01-01":
                        NotDeportedUntil_1_1_1943 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1944":
                    case "Present at 1944-01-01":
                        NotDeportedUntil_1_1_1944 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 1. 1. 1945":
                    case "Present at 1945-01-01":
                        NotDeportedUntil_1_1_1945 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Nedeportováno k 9. 5. 1945":
                    case "Present at 1945-05-09":
                        NotDeportedUntil_9_5_1945 = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? int.Parse(element.Attribute("value")?.Value) : null;
                        break;
                    case "Název":
                    case "Label":
                        Title = element.Attribute("value")?.Value;
                        break;
                    case "Popis":
                    case "Description":
                        Description = element.Attribute("value")?.Value;
                        break;
                    case "Typ":
                    case "Type":
                        Type = element.Attribute("value")?.Value;
                        break;
                    case "Specifikace":
                    case "Specification":
                        Specification = element.Attribute("value")?.Value;
                        break;
                    case "Typ (alternativní)":
                    case "Type (alternative)":
                        TypeAlternative = element.Attribute("value")?.Value;
                        break;
                    case "Specifikace (alternativní)":
                    case "Specification (alternative)":
                        SpecificationAlternative = element.Attribute("value")?.Value;
                        break;
                    case "Datum":
                    case "Date":
                        Date = !string.IsNullOrEmpty(element.Attribute("value")?.Value) ? DateTime.ParseExact(element.Attribute("value")?.Value, "dd.MM.yyyy", CultureInfo.InvariantCulture) : null;
                        break;
                    case "Místo":
                    case "Place":
                        Place = element.Attribute("value")?.Value;
                        break;
                    case "Dokumenty":
                    case "Documents":
                        Documents = element.Attribute("value")?.Value;
                        break;
                }
                if (string.IsNullOrEmpty(Address) && !string.IsNullOrEmpty(AddressCzechOccupation))
                    Address = new Regex("/.*").Replace(AddressCzechOccupation, "");
            }
        }


        public string? NameAndBirthDate { get; set; }
        public string? Details { get; set; }
        public string? Address { get; set; }
        public string? AddressCzechOccupation { get; set; }
        public string? AddressGermanOccupation { get; set; }
        public int? JewishResidents1941 { get; set; }
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
