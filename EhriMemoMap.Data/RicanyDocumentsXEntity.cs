using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyDocumentsXEntity
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long EntityId { get; set; }

    public virtual RicanyDocument Document { get; set; } = null!;

    public virtual RicanyEntity Entity { get; set; } = null!;
}
