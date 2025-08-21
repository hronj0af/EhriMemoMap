using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class EntitiesXMedium
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long MediaId { get; set; }

    public virtual Entity Entity { get; set; } = null!;

    public virtual Medium Media { get; set; } = null!;
}
