using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data.Memogis;

public partial class Entity
{
    public long Id { get; set; }

    public int? EntityId { get; set; }

    public string? Label { get; set; }

    public string? DetailsCs { get; set; }

    public string? DetailsEn { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public int? PlaceId { get; set; }

    public DateTime? TransportDate { get; set; }

    public string? Photo { get; set; }

    public virtual ICollection<DocumentsXEntity> DocumentsXEntities { get; set; } = new List<DocumentsXEntity>();
    public virtual ICollection<EntitiesXMedium> EntitiesXMedia { get; set; } = new List<EntitiesXMedium>();
    public virtual ICollection<LastResidence> LastResidences { get; set; } = new List<LastResidence>();

}
