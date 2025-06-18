using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyListItem
{
    public long Id { get; set; }

    public string ListType { get; set; } = null!;

    public string LabelCs { get; set; } = null!;

    public string LabelEn { get; set; } = null!;

    public virtual ICollection<RicanyEntity> RicanyEntityFateNavigations { get; set; } = new List<RicanyEntity>();

    public virtual ICollection<RicanyEntity> RicanyEntitySexNavigations { get; set; } = new List<RicanyEntity>();

    public virtual ICollection<RicanyNarrativeMap> RicanyNarrativeMaps { get; set; } = [];
}
