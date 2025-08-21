using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyEvent
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public DateTime? Date { get; set; }

    public string? LinkCs { get; set; }

    public string? LinkEn { get; set; }

    public virtual ICollection<RicanyEventsXPlace> RicanyEventsXPlaces { get; set; } = new List<RicanyEventsXPlace>();
}
