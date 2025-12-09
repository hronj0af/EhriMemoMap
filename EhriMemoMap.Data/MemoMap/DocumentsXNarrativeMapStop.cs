using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class DocumentsXNarrativeMapStop
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long NarrativeMapStopId { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual NarrativeMapStop NarrativeMapStop { get; set; } = null!;
}
