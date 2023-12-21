using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PragueIncidentsDocument
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public string? DocumentCs { get; set; }
    public string? DocumentEn { get; set; }
    public string? Img { get; set; }
}
