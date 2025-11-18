using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyPoi
{
    public long Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public long PlaceId { get; set; }

    public virtual ICollection<RicanyDocumentsXPoi> RicanyDocumentsXPois { get; set; } = new List<RicanyDocumentsXPoi>();

    public virtual RicanyPlace Place { get; set; } = null!;
}
