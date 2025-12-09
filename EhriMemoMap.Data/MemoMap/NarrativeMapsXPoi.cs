using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class NarrativeMapsXPoi
{
    public long Id { get; set; }

    public long NarrativeMapId { get; set; }

    public long PoiId { get; set; }

    public virtual NarrativeMap NarrativeMap { get; set; } = null!;

    public virtual Poi Poi { get; set; } = null!;
}
