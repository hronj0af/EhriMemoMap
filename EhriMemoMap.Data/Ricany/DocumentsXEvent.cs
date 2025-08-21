using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class DocumentsXEvent
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long EventId { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
