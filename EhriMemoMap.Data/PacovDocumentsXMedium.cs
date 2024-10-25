using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovDocumentsXMedium
{
    public long Id { get; set; }

    public long DocumentsId { get; set; }

    public long MediaId { get; set; }

    public virtual PacovDocument Documents { get; set; } = null!;

    public virtual PacovMedium Media { get; set; } = null!;
}
