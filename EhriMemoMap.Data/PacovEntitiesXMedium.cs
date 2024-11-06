using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovEntitiesXMedium
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long MediumId { get; set; }

    public virtual PacovEntity Entity { get; set; } = null!;

    public virtual PacovMedium Medium { get; set; } = null!;
}
