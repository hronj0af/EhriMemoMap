using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovDocumentsXPoi
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long PoiId { get; set; }

    public virtual PacovDocument Document { get; set; } = null!;

    public virtual PacovPoi Poi { get; set; } = null!;
}
