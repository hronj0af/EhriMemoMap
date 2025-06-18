using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyMedium
{
    public long Id { get; set; }

    public int? OmekaId { get; set; }

    public string? OmekaUrl { get; set; }

    public virtual ICollection<RicanyDocumentsXMedium> RicanyDocumentsXMedia { get; set; } = new List<RicanyDocumentsXMedium>();

    public virtual ICollection<RicanyEntitiesXMedium> RicanyEntitiesXMedia { get; set; } = new List<RicanyEntitiesXMedium>();
}
