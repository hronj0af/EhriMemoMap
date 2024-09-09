namespace EhriMemoMap.Client.Helpers
{
    public static class ParseHelpers
    {
        public static string? ConvertXMLNameAttributesToParsableValues(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            foreach (var item in NameAttributeMapping())
                text = text.Replace(@$"name=""{item.Key}""", $@"name=""{item.Value}""");
            return text;
        }

        public static Dictionary<string, string> NameAttributeMapping()
        {
            return new Dictionary<string, string>
            {
                { "Jméno (datum narození)", "NameAndBirthDate" },
                { "Detaily", "Details" },
                { "Adresa", "Address" },
                { "Adresa (česky, z doby okupace)", "AddressCzechOccupation" },
                { "Adresa (německy, z doby okupace)", "AddressGermanOccupation" },
                { "Počet židovských obyvatel (1. 10. 1941)", "JewsCitizens1942" },
                { "Zavražděno", "Murdered" },
                { "Přežilo", "Survived" },
                { "Osud neznámý", "FateUnknown" },
                { "Nedeportováno k 1. 1. 1942", "NotDeportedUntil_1_1_1942" },
                { "Nedeportováno k 1. 1. 1943", "NotDeportedUntil_1_1_1943" },
                { "Nedeportováno k 1. 1. 1944", "NotDeportedUntil_1_1_1944" },
                { "Nedeportováno k 1. 1. 1945", "NotDeportedUntil_1_1_1945" },
                { "Nedeportováno k 9. 5. 1945", "NotDeportedUntil_9_5_1945" },
                { "Název", "Title" },
                { "Popis", "Description" },
                { "Typ", "Type" },
                { "Specifikace", "Specification" },
                { "Typ (alternativní)", "TypeAlternative" },
                { "Specifikace (alternativní)", "SpecificationAlternative" },
                { "Datum", "Date" },
                { "Místo", "Place" },
                { "Dokumenty", "Documents" },
            };
        }
    }
}
