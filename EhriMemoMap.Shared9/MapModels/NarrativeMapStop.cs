namespace EhriMemoMap.Shared;

public class NarrativeMapStop
{
    public long Id { get; set; }

    public int? StopOrder { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DateCs { get; set; }

    public string? DateEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }
    public Document[]? Documents { get; set; } = [];
    public Place[]? Places { get; set; } = [];

}