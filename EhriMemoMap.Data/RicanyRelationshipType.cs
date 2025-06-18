using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyRelationshipType
{
    public long Id { get; set; }

    public string Table { get; set; } = null!;

    public string LabelCs { get; set; } = null!;

    public string LabelEn { get; set; } = null!;

    public virtual ICollection<RicanyEntitiesXEntity> RicanyEntitiesXEntities { get; set; } = new List<RicanyEntitiesXEntity>();

    public virtual ICollection<RicanyEntitiesXPlace> RicanyEntitiesXPlaces { get; set; } = new List<RicanyEntitiesXPlace>();

    public virtual ICollection<RicanyNarrativeMapStopsXPlace> RicanyNarrativeMapStopsXPlaces { get; set; } = [];

}
