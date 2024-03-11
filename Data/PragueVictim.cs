using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EhriMemoMap.Data;

[Table("prague_victims")]
public partial class PragueVictim
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("entity_id")]
    public int? EntityId { get; set; }

    [Column("label")]
    public string? Label { get; set; }

    [Column("details_cs")]
    public string? DetailsCs { get; set; }

    [Column("details_en")]
    public string? DetailsEn { get; set; }

    [Column("address_cs")]
    public string? AddressCs { get; set; }

    [Column("address_en")]
    public string? AddressEn { get; set; }

    [Column("place_id")]
    public int? PlaceId { get; set; }

    [ForeignKey("PlaceId")]
    [InverseProperty("PragueVictims")]
    public virtual PragueAddressesStat? Place { get; set; }
}
