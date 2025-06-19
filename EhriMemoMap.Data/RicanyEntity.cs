using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class RicanyEntity
{
    public long Id { get; set; }

    public string? Idno { get; set; }

    public long? IdIti { get; set; }

    public string? Surname { get; set; }

    public string? Firstname { get; set; }

    public string? Maidenname { get; set; }

    public string? Title { get; set; }

    public long? Sex { get; set; }

    public DateTime? Birthdate { get; set; }

    public DateTime? Deathdate { get; set; }

    public long? Fate { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public virtual RicanyListItem? FateNavigation { get; set; }

    public virtual ICollection<RicanyDocumentsXEntity> RicanyDocumentsXEntities { get; set; } = new List<RicanyDocumentsXEntity>();

    public virtual ICollection<RicanyEntitiesXEntity> RicanyEntitiesXEntityEntity1s { get; set; } = new List<RicanyEntitiesXEntity>();

    public virtual ICollection<RicanyEntitiesXEntity> RicanyEntitiesXEntityEntity2s { get; set; } = new List<RicanyEntitiesXEntity>();

    public virtual ICollection<RicanyEntitiesXMedium> RicanyEntitiesXMedia { get; set; } = new List<RicanyEntitiesXMedium>();

    public virtual ICollection<RicanyEntitiesXNarrativeMap> RicanyEntitiesXNarrativeMaps { get; set; } = new List<RicanyEntitiesXNarrativeMap>();

    public virtual ICollection<RicanyEntitiesXPlace> RicanyEntitiesXPlaces { get; set; } = new List<RicanyEntitiesXPlace>();

    public virtual ICollection<RicanyEntitiesXTransport> RicanyEntitiesXTransports { get; set; } = new List<RicanyEntitiesXTransport>();

    public virtual RicanyListItem? SexNavigation { get; set; }
}
