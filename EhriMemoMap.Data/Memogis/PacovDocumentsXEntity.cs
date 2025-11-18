using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovDocumentsXEntity
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long EntityId { get; set; }

    public virtual PacovDocument Document { get; set; } = null!;

    public virtual PacovEntity Entity { get; set; } = null!;
}
