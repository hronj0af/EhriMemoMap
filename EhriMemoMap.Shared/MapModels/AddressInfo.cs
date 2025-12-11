using System.Text;

namespace EhriMemoMap.Shared
{
    public class AddressInfo
    {
        public long Id { get; set; }
        public string? Cs { get; set; }
        public string? En { get; set; }
        public string? De { get; set; }
        public string? CurrentCs { get; set; }
        public string? CurrentEn { get; set; }
        public long? Type { get; set; }
        public string? TypeCs { get; set; }
        public string? TypeEn { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public string GetAddressWithDates(string cultureName)
        {

            var address = new StringBuilder(cultureName == "en-US" ? En : Cs);
            if (DateFrom.HasValue || DateTo.HasValue)
            {
                address.Append(" (");
                if (DateFrom.HasValue && DateTo.HasValue && DateFrom.Value.Year != DateTo.Value.Year)
                    address.Append(DateFrom.Value.Year + "-" + DateTo.Value.Year);
                else if (DateFrom.HasValue && DateTo.HasValue && DateFrom.Value.Year == DateTo.Value.Year)
                    address.Append(DateFrom.Value.Year);
                else if (DateTo.HasValue)
                    address.Append((cultureName == "en-US" ? "until " : "do ") + DateTo.Value.Year);
                else if (DateFrom.HasValue)
                    address.Append((cultureName == "en-US" ? "after " : "po ") + DateFrom.Value.Year);
                address.Append(')');
            }
            return address.ToString();
        }
        public string? GetAddress(string cultureName)
        {
            return cultureName == "en-US" ? En : Cs;
        }

    }
}
