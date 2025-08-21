using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class Medium
{
    public long Id { get; set; }

    public int? OmekaId { get; set; }

    public string? OmekaUrl { get; set; }

    public virtual ICollection<DocumentsXMedium> DocumentsXMedia { get; set; } = new List<DocumentsXMedium>();

    public virtual ICollection<EntitiesXMedium> EntitiesXMedia { get; set; } = new List<EntitiesXMedium>();
}
