using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyDocumentsXPoi
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long PoiId { get; set; }

    public virtual RicanyDocument Document { get; set; } = null!;

    public virtual RicanyPoi Poi { get; set; } = null!;
}
