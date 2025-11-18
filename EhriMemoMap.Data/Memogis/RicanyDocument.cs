using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyDocument
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public int? OmekaId { get; set; }

    public string? OmekaIdno { get; set; }

    public string? OmekaUrl { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public string? CreationDateCs { get; set; }

    public string? CreationDateEn { get; set; }

    public long? CreationPlace { get; set; }

    public string? Owner { get; set; }

    public virtual RicanyPlace? CreationPlaceNavigation { get; set; }

    public virtual ICollection<RicanyDocumentsXEntity> RicanyDocumentsXEntities { get; set; } = [];

    public virtual ICollection<RicanyDocumentsXMedium> RicanyDocumentsXMedia { get; set; } = [];

    public virtual ICollection<RicanyDocumentsXPoi> RicanyDocumentsXPois { get; set; } = [];
    public virtual ICollection<RicanyDocumentsXNarrativeMapStop> RicanyDocumentsXNarrativeMapStops { get; set; } = [];

}
