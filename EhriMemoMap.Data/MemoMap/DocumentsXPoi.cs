using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class DocumentsXPoi
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long PoiId { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Poi Poi { get; set; } = null!;
}
