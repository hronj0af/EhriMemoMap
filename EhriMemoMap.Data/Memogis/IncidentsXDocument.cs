using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Memogis;

public partial class IncidentsXDocument
{
    public long Id { get; set; }

    public long IncidentId { get; set; }

    public string DocumentCs { get; set; } = null!;

    public string DocumentEn { get; set; } = null!;

    public string Img { get; set; } = null!;
}
