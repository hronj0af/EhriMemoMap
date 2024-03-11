using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

[Table("prague_incidents")]
public partial class PragueIncident
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("label_cs")]
    public string? LabelCs { get; set; }

    [Column("label_en")]
    public string? LabelEn { get; set; }

    [Column("description_cs")]
    public string? DescriptionCs { get; set; }

    [Column("description_en")]
    public string? DescriptionEn { get; set; }

    [Column("type_1_cs")]
    public string? Type1Cs { get; set; }

    [Column("type_1_en")]
    public string? Type1En { get; set; }

    [Column("spec_1_cs")]
    public string? Spec1Cs { get; set; }

    [Column("spec_1_en")]
    public string? Spec1En { get; set; }

    [Column("type_2_cs")]
    public string? Type2Cs { get; set; }

    [Column("type_2_en")]
    public string? Type2En { get; set; }

    [Column("spec_2_cs")]
    public string? Spec2Cs { get; set; }

    [Column("spec_2_en")]
    public string? Spec2En { get; set; }

    [Column("date")]
    public string? Date { get; set; }

    [Column("place_cs")]
    public string? PlaceCs { get; set; }

    [Column("place_en")]
    public string? PlaceEn { get; set; }

    [Column("documents_cs")]
    public string? DocumentsCs { get; set; }

    [Column("documents_en")]
    public string? DocumentsEn { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }
}
