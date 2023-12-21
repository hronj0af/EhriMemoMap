using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PragueIncident
{
    public int Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public string? Type1Cs { get; set; }

    public string? Type1En { get; set; }

    public string? Spec1Cs { get; set; }

    public string? Spec1En { get; set; }

    public string? Type2Cs { get; set; }

    public string? Type2En { get; set; }

    public string? Spec2Cs { get; set; }

    public string? Spec2En { get; set; }

    public string? Date { get; set; }

    public string? PlaceCs { get; set; }

    public string? PlaceEn { get; set; }

    public string? DocumentsCs { get; set; }

    public string? DocumentsEn { get; set; }

    public Geometry? Geography { get; set; }

}
