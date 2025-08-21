﻿using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class Poi
{
    public long Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public long PlaceId { get; set; }

    public virtual ICollection<DocumentsXPoi> DocumentsXPois { get; set; } = new List<DocumentsXPoi>();

    public virtual Place Place { get; set; } = null!;
}
