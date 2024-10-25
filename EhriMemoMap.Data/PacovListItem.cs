using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovListItem
{
    public long Id { get; set; }

    public string ListType { get; set; } = null!;

    public string LabelCs { get; set; } = null!;

    public string LabelEn { get; set; } = null!;

    public virtual ICollection<PacovEntity> PacovEntityFateNavigations { get; set; } = new List<PacovEntity>();

    public virtual ICollection<PacovEntity> PacovEntitySexNavigations { get; set; } = new List<PacovEntity>();
}
