using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovRelationshipType
{
    public long Id { get; set; }

    public string Table { get; set; } = null!;

    public string LabelCs { get; set; } = null!;

    public string LabelEn { get; set; } = null!;

    public virtual ICollection<PacovEntitiesXEntity> PacovEntitiesXEntities { get; set; } = new List<PacovEntitiesXEntity>();

    public virtual ICollection<PacovEntitiesXPlace> PacovEntitiesXPlaces { get; set; } = new List<PacovEntitiesXPlace>();

    public virtual ICollection<PacovNarrativeMapStopsXPlace> PacovNarrativeMapStopsXPlaces { get; set; } = [];

}
