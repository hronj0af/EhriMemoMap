using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovEntitiesXMedium
{
    public long Id { get; set; }

    public long EntitiesId { get; set; }

    public long MediaId { get; set; }

    public virtual PacovEntity Entities { get; set; } = null!;

    public virtual PacovMedium Media { get; set; } = null!;
}
