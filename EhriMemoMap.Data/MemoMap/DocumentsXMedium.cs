using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class DocumentsXMedium
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long MediaId { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Medium Media { get; set; } = null!;
}
