using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovMedium
{
    public long Id { get; set; }

    public int? OmekaId { get; set; }

    public string? OmekaUrl { get; set; }

    public virtual ICollection<PacovDocumentsXMedium> PacovDocumentsXMedia { get; set; } = new List<PacovDocumentsXMedium>();

    public virtual ICollection<PacovEntitiesXMedium> PacovEntitiesXMedia { get; set; } = new List<PacovEntitiesXMedium>();
}
