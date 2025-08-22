namespace EhriMemoMap.Shared;

public partial class Document
{
    public Document()
    {
    }

    public long Id { get; set; }

    public string? Type { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public string? CreationDateCs { get; set; }

    public string? CreationDateEn { get; set; }

    public string? CreationPlaceCs { get; set; }
    public string? CreationPlaceEn { get; set; }

    public string? Owner { get; set; }
    public OmekaUrl[]? Url { get; set; }
    public string? DocumentUrlCs { get; set; }
    public string? DocumentUrlEn { get; set; }

    public string? GetTitle(string lang)
    {
        return lang switch
        {
            "cs-CZ" => LabelCs +
                    (!string.IsNullOrEmpty(DescriptionCs) ? " | " + DescriptionCs : "") +
                    (!string.IsNullOrEmpty(CreationDateCs) ? " | " + CreationDateCs : "") +
                    (!string.IsNullOrEmpty(CreationPlaceCs) ? " | " + CreationPlaceCs : "") +
                    (!string.IsNullOrEmpty(Owner) ? " | " + Owner : ""),

            _ => LabelEn + 
                    (!string.IsNullOrEmpty(DescriptionEn) ? " | " + DescriptionEn : "") + 
                    (!string.IsNullOrEmpty(CreationDateEn) ? " | " + CreationDateEn : "") + 
                    (!string.IsNullOrEmpty(CreationPlaceEn) ? " | " + CreationPlaceEn : "") + 
                    (!string.IsNullOrEmpty(Owner) ? " | " + Owner : "")
        };
    }
}

public class OmekaUrl(string? url, string? thumbnailUrl)
{
    public string? Url { get; set; } = url;
    public string? ThumbnailUrl { get; set; } = thumbnailUrl;
}
