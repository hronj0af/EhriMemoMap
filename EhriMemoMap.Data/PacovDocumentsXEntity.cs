using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovDocumentsXEntity
{
    public long Id { get; set; }

    public long DocumentsId { get; set; }

    public long EntitiesId { get; set; }

    public virtual PacovDocument Documents { get; set; } = null!;

    public virtual PacovEntity Entities { get; set; } = null!;
}
