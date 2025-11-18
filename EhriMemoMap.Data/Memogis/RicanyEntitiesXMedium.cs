using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyEntitiesXMedium
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long MediumId { get; set; }

    public virtual RicanyEntity Entity { get; set; } = null!;

    public virtual RicanyMedium Medium { get; set; } = null!;
}
