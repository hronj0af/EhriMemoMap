using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovDocumentsXPoi
{
    public long Id { get; set; }

    public long DocumentsId { get; set; }

    public long PoisId { get; set; }

    public virtual PacovDocument Documents { get; set; } = null!;

    public virtual PacovPoi Pois { get; set; } = null!;
}
