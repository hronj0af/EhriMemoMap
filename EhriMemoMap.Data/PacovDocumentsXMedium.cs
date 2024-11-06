using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovDocumentsXMedium
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long MediumId { get; set; }

    public virtual PacovDocument Document { get; set; } = null!;

    public virtual PacovMedium Medium { get; set; } = null!;
}
