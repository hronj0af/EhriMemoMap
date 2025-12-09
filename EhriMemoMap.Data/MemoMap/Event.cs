using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data.MemoMap;

public partial class Event
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

    public virtual ICollection<DocumentsXEvent> DocumentsXEvents { get; set; } = new List<DocumentsXEvent>();

    public virtual ICollection<EventsXPlace> EventsXPlaces { get; set; } = new List<EventsXPlace>();
}
