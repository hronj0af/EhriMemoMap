using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class DocumentsXPlacesOfMemory
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long PlaceOfMemoryId { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual PlacesOfMemory PlaceOfMemory { get; set; } = null!;
}
