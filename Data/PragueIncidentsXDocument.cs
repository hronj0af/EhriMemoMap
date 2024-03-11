using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EhriMemoMap.Data;

[Table("prague_incidents_x_documents")]
[Index("IncidentId", Name = "fki_prague_incidents_x_documents_incident_id_ref_prague_inciden")]
public partial class PragueIncidentsXDocument
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("incident_id")]
    public long IncidentId { get; set; }

    [Column("document_cs")]
    public string DocumentCs { get; set; } = null!;

    [Column("document_en")]
    public string DocumentEn { get; set; } = null!;

    [Column("img")]
    public string Img { get; set; } = null!;
}
