namespace EhriMemoMap.Shared;

public class NarrativeMap
{
    public long Id { get; set; }

    public long Type { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public NarrativeMapStop[]? Stops { get; set; } = [];

}