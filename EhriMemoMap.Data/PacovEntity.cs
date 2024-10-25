using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class PacovEntity
{
    public long Id { get; set; }

    public string Idno { get; set; } = null!;

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

    public virtual PacovListItem? FateNavigation { get; set; }

    public virtual ICollection<PacovDocumentsXEntity> PacovDocumentsXEntities { get; set; } = new List<PacovDocumentsXEntity>();

    public virtual ICollection<PacovEntitiesXEntity> PacovEntitiesXEntityEntity1s { get; set; } = new List<PacovEntitiesXEntity>();

    public virtual ICollection<PacovEntitiesXEntity> PacovEntitiesXEntityEntity2s { get; set; } = new List<PacovEntitiesXEntity>();

    public virtual ICollection<PacovEntitiesXMedium> PacovEntitiesXMedia { get; set; } = new List<PacovEntitiesXMedium>();

    public virtual ICollection<PacovEntitiesXPlace> PacovEntitiesXPlaces { get; set; } = new List<PacovEntitiesXPlace>();

    public virtual ICollection<PacovEntitiesXTransport> PacovEntitiesXTransports { get; set; } = new List<PacovEntitiesXTransport>();

    public virtual PacovListItem? SexNavigation { get; set; }
}
