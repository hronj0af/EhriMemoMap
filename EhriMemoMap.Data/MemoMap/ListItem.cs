using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class ListItem
{
    public long Id { get; set; }

    public string ListType { get; set; } = null!;

    public string LabelCs { get; set; } = null!;

    public string LabelEn { get; set; } = null!;

    public virtual ICollection<Entity> EntityFateNavigations { get; set; } = new List<Entity>();

    public virtual ICollection<Entity> EntitySexNavigations { get; set; } = new List<Entity>();

    public virtual ICollection<NarrativeMap> NarrativeMaps { get; set; } = new List<NarrativeMap>();
}
