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

            StatisticsAbsent = new List<WMSFeatureInfoStatistics>();
            StatisticsPresent = new List<WMSFeatureInfoStatistics>();

            foreach (var element in xdoc.Descendants("Attribute"))
            {
                var elementName = element.Attribute("name")?.Value;
                switch (elementName)
                {
                    case "Jméno (datum narození)":
                    case "Victim":
                        NameAndBirthDate = element.Attribute("value")?.Value;
                        break;
                    case "Fotografie":
                    case "Photo":
                        Photo = element.Attribute("value")?.Value;
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
                        StatisticsPresent.Add(new WMSFeatureInfoStatistics { Date = new DateTime(1941,10,1), Number = int.Parse(element.Attribute("value")?.Value) });
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
                    case { } when elementName.StartsWith("Nedeportováno k"):
                    case { } when elementName.StartsWith("Present at"):
                        var datePresent = DateTime.ParseExact(elementName.Replace("Nedeportováno k ", "").Replace("Present at ", ""), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        StatisticsPresent.Add(new WMSFeatureInfoStatistics { Date = datePresent, Number = int.Parse(element.Attribute("value")?.Value) });
                        break;
                    case { } when elementName.StartsWith("Absent at"):
                        var dateAbsent = DateTime.ParseExact(elementName.Replace("Absent at ", ""), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        StatisticsAbsent.Add(new WMSFeatureInfoStatistics { Date = dateAbsent, Number = int.Parse(element.Attribute("value")?.Value) });
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
                    case "Status":
                        Status = Enum.Parse<StatusEnum>(element.Attribute("value")?.Value);
                        break;
                }

                // pro bydliště je v adrese často i číslo popisné, které ovšem není u adresy oběti,
                // tak to tady musím sjednotit (ale mělo by to být jednotné už ve výsledku dotazu - dodělat!!!
                if (string.IsNullOrEmpty(Address) && !string.IsNullOrEmpty(AddressCzechOccupation))
                    Address = new Regex("/.*").Replace(AddressCzechOccupation, "");
            }
        }


        public string? NameAndBirthDate { get; set; }
        public string? Photo { get; set; }
        public string? Details { get; set; }
        public string? Address { get; set; }
        public string? AddressCzechOccupation { get; set; }
        public string? AddressGermanOccupation { get; set; }
        public int? Murdered { get; set; }
        public int? Survived { get; set; }
        public int? FateUnknown { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Specification { get; set; }
        public string? TypeAlternative { get; set; }
        public string? SpecificationAlternative { get; set; }
        public DateTime? Date { get; set; }
        public string? Place { get; set; }
        public string? Documents { get; set; }
        public StatusEnum? Status { get; set; }
        public WMSFeatureType? FeatureInfoType { get; set; }
        public List<WMSFeatureInfoStatistics> StatisticsAbsent { get; set; }
        public List<WMSFeatureInfoStatistics> StatisticsPresent { get; set; }

    }

    public enum StatusEnum
    {
        present, absent
    }

    public class WMSFeatureInfoStatistics
    {
        public DateTime Date { get; set; }
        public int Number { get; set; }
    }

}
