using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyPlaceOfMemory
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? InscriptionCs { get; set; }

    public string? InscriptionEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public DateTime? CreationDate { get; set; }

    public virtual ICollection<RicanyPlacesOfMemoryXPlaceOfMemory> RicanyPlacesOfMemoryXPlaceOfMemoryPlaceOfMemory1s { get; set; } = new List<RicanyPlacesOfMemoryXPlaceOfMemory>();

    public virtual ICollection<RicanyPlacesOfMemoryXPlaceOfMemory> RicanyPlacesOfMemoryXPlaceOfMemoryPlaceOfMemory2s { get; set; } = new List<RicanyPlacesOfMemoryXPlaceOfMemory>();

    public virtual ICollection<RicanyPlacesXPlaceOfMemory> RicanyPlacesXPlacesOfMemory { get; set; } = new List<RicanyPlacesXPlaceOfMemory>();
}
