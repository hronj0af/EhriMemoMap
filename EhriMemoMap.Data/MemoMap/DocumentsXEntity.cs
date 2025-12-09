using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class DocumentsXEntity
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long EntityId { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Entity Entity { get; set; } = null!;
}
