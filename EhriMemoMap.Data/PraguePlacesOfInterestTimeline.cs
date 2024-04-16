﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NodaTime;

namespace EhriMemoMap.Data;

[Keyless]
[Table("prague_places_of_interest_timeline")]
public partial class PraguePlacesOfInterestTimeline
{
    [Column("id")]
    public int? Id { get; set; }

    [Column("label_cs")]
    public string? LabelCs { get; set; }

    [Column("label_en")]
    public string? LabelEn { get; set; }

    [Column("address_cs")]
    public string? AddressCs { get; set; }

    [Column("address_en")]
    public string? AddressEn { get; set; }

    [Column("quarter_1938")]
    public string? Quarter1938 { get; set; }

    [Column("description_cs")]
    public string? DescriptionCs { get; set; }

    [Column("description_en")]
    public string? DescriptionEn { get; set; }

    [Column("inaccessible")]
    public bool? Inaccessible { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }

    [Column("geography_polygon")]
    public Geometry? GeographyPolygon { get; set; }

    [Column("date_from")]
    public DateTime? DateFrom { get; set; }

    [Column("date_to")]
    public DateTime? DateTo { get; set; }
}