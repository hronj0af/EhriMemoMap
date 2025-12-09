using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data.MemoMap;

public partial class Entity
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

    public string? BirthdateText { get; set; }

    public string? DeathdateText { get; set; }
    public long? Fate { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public virtual ICollection<DocumentsXEntity> DocumentsXEntities { get; set; } = new List<DocumentsXEntity>();

    public virtual ICollection<EntitiesXEntity> EntitiesXEntityEntity1s { get; set; } = new List<EntitiesXEntity>();

    public virtual ICollection<EntitiesXEntity> EntitiesXEntityEntity2s { get; set; } = new List<EntitiesXEntity>();

    public virtual ICollection<EntitiesXMedium> EntitiesXMedia { get; set; } = new List<EntitiesXMedium>();

    public virtual ICollection<EntitiesXNarrativeMap> EntitiesXNarrativeMaps { get; set; } = new List<EntitiesXNarrativeMap>();

    public virtual ICollection<EntitiesXPlace> EntitiesXPlaces { get; set; } = new List<EntitiesXPlace>();

    public virtual ICollection<EntitiesXTransport> EntitiesXTransports { get; set; } = new List<EntitiesXTransport>();

    public virtual ListItem? FateNavigation { get; set; }

    public virtual ListItem? SexNavigation { get; set; }
}
