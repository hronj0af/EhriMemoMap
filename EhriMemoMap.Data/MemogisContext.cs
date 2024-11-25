using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EhriMemoMap.Data;

public partial class MemogisContext : DbContext
{
    public MemogisContext()
    {
    }

    public MemogisContext(DbContextOptions<MemogisContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(x => x
                .UseNodaTime()
                .UseNetTopologySuite());

    public virtual DbSet<MapObject> MapObjects { get; set; }

    public virtual DbSet<MapStatistic> MapStatistics { get; set; }
    
    public virtual DbSet<PacovDocument> PacovDocuments { get; set; }

    public virtual DbSet<PacovDocumentsXEntity> PacovDocumentsXEntities { get; set; }

    public virtual DbSet<PacovDocumentsXMedium> PacovDocumentsXMedia { get; set; }

    public virtual DbSet<PacovDocumentsXNarrativeMapStop> PacovDocumentsXNarrativeMapStops { get; set; }

    public virtual DbSet<PacovDocumentsXPoi> PacovDocumentsXPois { get; set; }

    public virtual DbSet<PacovEntitiesXEntity> PacovEntitiesXEntities { get; set; }

    public virtual DbSet<PacovEntitiesXMedium> PacovEntitiesXMedia { get; set; }

    public virtual DbSet<PacovEntitiesXNarrativeMap> PacovEntitiesXNarrativeMaps { get; set; }

    public virtual DbSet<PacovEntitiesXPlace> PacovEntitiesXPlaces { get; set; }

    public virtual DbSet<PacovEntitiesXTransport> PacovEntitiesXTransports { get; set; }

    public virtual DbSet<PacovEntity> PacovEntities { get; set; }

    public virtual DbSet<PacovListItem> PacovListItems { get; set; }

    public virtual DbSet<PacovMedium> PacovMedia { get; set; }

    public virtual DbSet<PacovNarrativeMap> PacovNarrativeMaps { get; set; }

    public virtual DbSet<PacovNarrativeMapStop> PacovNarrativeMapStops { get; set; }

    public virtual DbSet<PacovNarrativeMapStopsXPlace> PacovNarrativeMapStopsXPlaces { get; set; }

    public virtual DbSet<PacovNarrativeMapXNarrativeMapStop> PacovNarrativeMapXNarrativeMapStops { get; set; }
    
    public virtual DbSet<PacovPlace> PacovPlaces { get; set; }

    public virtual DbSet<PacovPoi> PacovPois { get; set; }

    public virtual DbSet<PacovRelationshipType> PacovRelationshipTypes { get; set; }

    public virtual DbSet<PacovTransport> PacovTransports { get; set; }

    public virtual DbSet<PragueAddressesStat> PragueAddressesStats { get; set; }

    public virtual DbSet<PragueAddressesStatsTimeline> PragueAddressesStatsTimelines { get; set; }

    public virtual DbSet<PragueIncident> PragueIncidents { get; set; }

    public virtual DbSet<PragueIncidentsTimeline> PragueIncidentsTimelines { get; set; }

    public virtual DbSet<PragueIncidentsXDocument> PragueIncidentsXDocuments { get; set; }

    public virtual DbSet<PragueLastResidence> PragueLastResidences { get; set; }

    public virtual DbSet<PraguePlacesOfInterest> PraguePlacesOfInterests { get; set; }

    public virtual DbSet<PraguePlacesOfInterestTimeline> PraguePlacesOfInterestTimelines { get; set; }

    public virtual DbSet<PraguePlacesOfMemory> PraguePlacesOfMemories { get; set; }

    public virtual DbSet<PragueQuartersStat> PragueQuartersStats { get; set; }

    public virtual DbSet<PragueQuartersStatsTimeline> PragueQuartersStatsTimelines { get; set; }

    public virtual DbSet<PragueVictim> PragueVictims { get; set; }

    public virtual DbSet<PragueVictimsTimeline> PragueVictimsTimelines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<MapObject>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("map_objects");

            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Citizens).HasColumnName("citizens");
            entity.Property(e => e.CitizensTotal).HasColumnName("citizens_total");
            entity.Property(e => e.DateFrom).HasColumnName("date_from");
            entity.Property(e => e.DateTo).HasColumnName("date_to");
            entity.Property(e => e.GeographyMapPoint)
                .HasColumnType("geography")
                .HasColumnName("geography_map_point");
            entity.Property(e => e.GeographyMapPolygon).HasColumnName("geography_map_polygon");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LabelCs).HasColumnName("label_cs");
            entity.Property(e => e.LabelEn).HasColumnName("label_en");
            entity.Property(e => e.MapPoint).HasColumnName("map_point");
            entity.Property(e => e.MapPolygon).HasColumnName("map_polygon");
            entity.Property(e => e.PlaceType).HasColumnName("place_type");
        });

        modelBuilder.Entity<MapStatistic>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("map_statistics");

            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.DateFrom).HasColumnName("date_from");
            entity.Property(e => e.DateTo).HasColumnName("date_to");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.GeographyPolygon).HasColumnName("geography_polygon");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MapPoint).HasColumnName("map_point");
            entity.Property(e => e.MapPolygon).HasColumnName("map_polygon");
            entity.Property(e => e.QuarterCs).HasColumnName("quarter_cs");
            entity.Property(e => e.QuarterEn).HasColumnName("quarter_en");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.City).HasColumnName("city");

        });

        modelBuilder.Entity<PacovDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_documents_pkey");

            entity.ToTable("pacov_documents");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreationDateCs)
                .HasMaxLength(100)
                .HasColumnName("creation_date_cs");
            entity.Property(e => e.CreationDateEn)
                .HasMaxLength(100)
                .HasColumnName("creation_date_en");
            entity.Property(e => e.CreationPlace).HasColumnName("creation_place");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(250)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(250)
                .HasColumnName("label_en");
            entity.Property(e => e.OmekaId).HasColumnName("omeka_id");
            entity.Property(e => e.OmekaIdno)
                .HasMaxLength(25)
                .HasColumnName("omeka_idno");
            entity.Property(e => e.OmekaUrl)
                .HasMaxLength(250)
                .HasColumnName("omeka_url");
            entity.Property(e => e.Owner)
                .HasMaxLength(250)
                .HasColumnName("owner");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.CreationPlaceNavigation).WithMany(p => p.PacovDocuments)
                .HasForeignKey(d => d.CreationPlace)
                .HasConstraintName("creation_place_fkey");
        });

        modelBuilder.Entity<PacovDocumentsXEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_documents_x_entities_pkey");

            entity.ToTable("pacov_documents_x_entities");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");

            entity.HasOne(d => d.Document).WithMany(p => p.PacovDocumentsXEntities)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.Entity).WithMany(p => p.PacovDocumentsXEntities)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");
        });

        modelBuilder.Entity<PacovDocumentsXMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_documents_x_media_pkey");

            entity.ToTable("pacov_documents_x_media");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.MediumId).HasColumnName("media_id");

            entity.HasOne(d => d.Document).WithMany(p => p.PacovDocumentsXMedia)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.Medium).WithMany(p => p.PacovDocumentsXMedia)
                .HasForeignKey(d => d.MediumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("media_id_fkey");
        });

        modelBuilder.Entity<PacovDocumentsXNarrativeMapStop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_documents_x_narrative_map_stops_pkey");

            entity.ToTable("pacov_documents_x_narrative_map_stops");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");

            entity.Property(e => e.NarrativeMapStopId).HasColumnName("narrative_map_stop_id");

            entity.HasOne(d => d.Document).WithMany(p => p.PacovDocumentsXNarrativeMapStops)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.NarrativeMapStop).WithMany(p => p.PacovDocumentsXNarrativeMapStops)
                .HasForeignKey(d => d.NarrativeMapStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_stop_id_fkey");
        });

        modelBuilder.Entity<PacovDocumentsXPoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_documents_x_pois_pkey");

            entity.ToTable("pacov_documents_x_pois");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");

            entity.HasOne(d => d.Document).WithMany(p => p.PacovDocumentsXPois)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.Poi).WithMany(p => p.PacovDocumentsXPois)
                .HasForeignKey(d => d.PoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("poi_id_fkey");
        });

        modelBuilder.Entity<PacovEntitiesXEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_entities_x_entities_pkey");

            entity.ToTable("pacov_entities_x_entities");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Entity1Id).HasColumnName("entity_1_id");
            entity.Property(e => e.Entity2Id).HasColumnName("entity_2_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.Entity1).WithMany(p => p.PacovEntitiesXEntityEntity1s)
                .HasForeignKey(d => d.Entity1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_1_id_fkey");

            entity.HasOne(d => d.Entity2).WithMany(p => p.PacovEntitiesXEntityEntity2s)
                .HasForeignKey(d => d.Entity2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_2_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.PacovEntitiesXEntities)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationship_type_fkey");
        });

        modelBuilder.Entity<PacovEntitiesXMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_entities_x_media_pkey");

            entity.ToTable("pacov_entities_x_media");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.MediumId).HasColumnName("media_id");

            entity.HasOne(d => d.Entity).WithMany(p => p.PacovEntitiesXMedia)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.Medium).WithMany(p => p.PacovEntitiesXMedia)
                .HasForeignKey(d => d.MediumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("madia_id_fkey");
        });

        modelBuilder.Entity<PacovEntitiesXNarrativeMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_entities_x_narrative_maps_pkey");

            entity.ToTable("pacov_entities_x_narrative_maps");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.EntityId).HasColumnName("entity_id");

            entity.Property(e => e.NarrativeMapId).HasColumnName("narrative_map_id");

            entity.HasOne(d => d.Entity).WithMany(p => p.PacovEntitiesXNarrativeMaps)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.NarrativeMap).WithMany(p => p.PacovEntitiesXNarrativeMaps)
                .HasForeignKey(d => d.NarrativeMapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_id_fkey");

        });

        modelBuilder.Entity<PacovEntitiesXPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_entities_x_places_pkey");

            entity.ToTable("pacov_entities_x_places");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DateFrom).HasColumnName("date_from");
            entity.Property(e => e.DateTo).HasColumnName("date_to");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.Entity).WithMany(p => p.PacovEntitiesXPlaces)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.Place).WithMany(p => p.PacovEntitiesXPlaces)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.PacovEntitiesXPlaces)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationsip_type_fkey");
        });

        modelBuilder.Entity<PacovEntitiesXTransport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_entities_x_transports_pkey");

            entity.ToTable("pacov_entities_x_transports");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.NrInTransport).HasColumnName("nr_in_transport");
            entity.Property(e => e.TransportId).HasColumnName("transport_id");
            entity.Property(e => e.TransportOrder).HasColumnName("transport_order");

            entity.HasOne(d => d.Entity).WithMany(p => p.PacovEntitiesXTransports)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.Transport).WithMany(p => p.PacovEntitiesXTransports)
                .HasForeignKey(d => d.TransportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transport_id_fkey");
        });

        modelBuilder.Entity<PacovEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_entities_pkey");

            entity.ToTable("pacov_entities");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.Deathdate).HasColumnName("deathdate");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.Fate).HasColumnName("fate");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.IdIti).HasColumnName("id_iti");
            entity.Property(e => e.Idno)
                .HasMaxLength(25)
                .HasColumnName("idno");
            entity.Property(e => e.Maidenname)
                .HasMaxLength(50)
                .HasColumnName("maidenname");
            entity.Property(e => e.Sex).HasColumnName("sex");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");
            entity.Property(e => e.Title)
                .HasMaxLength(25)
                .HasColumnName("title");

            entity.HasOne(d => d.FateNavigation).WithMany(p => p.PacovEntityFateNavigations)
                .HasForeignKey(d => d.Fate)
                .HasConstraintName("fate_fkey");

            entity.HasOne(d => d.SexNavigation).WithMany(p => p.PacovEntitySexNavigations)
                .HasForeignKey(d => d.Sex)
                .HasConstraintName("sex_fkey");
        });

        modelBuilder.Entity<PacovListItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_list_items_pkey");

            entity.ToTable("pacov_list_items");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(100)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(100)
                .HasColumnName("label_en");
            entity.Property(e => e.ListType)
                .HasMaxLength(100)
                .HasColumnName("list_type");
        });

        modelBuilder.Entity<PacovMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_media_pkey");

            entity.ToTable("pacov_media");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.OmekaId).HasColumnName("omeka_id");
            entity.Property(e => e.OmekaUrl)
                .HasMaxLength(250)
                .HasColumnName("omeka_url");
        });

        modelBuilder.Entity<PacovNarrativeMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_narrative_maps_pkey");
            entity.ToTable("pacov_narrative_maps");
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DescritpionCs).HasColumnName("descritpion_cs");
            entity.Property(e => e.DescritpionEn).HasColumnName("descritpion_en");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(250)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(250)
                .HasColumnName("label_en");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.PacovNarrativeMaps)
                .HasForeignKey(d => d.Type)
                .HasConstraintName("type_fkey");
        });

        modelBuilder.Entity<PacovNarrativeMapStop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_narrative_map_stops_pkey");
            entity.ToTable("pacov_narrative_map_stops");
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DateCs).HasColumnName("date_cs");
            entity.Property(e => e.DateEn).HasColumnName("date_en");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(250)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(250)
                .HasColumnName("label_en");
            entity.Property(e => e.StopOrder).HasColumnName("stop_order");
        });

        modelBuilder.Entity<PacovNarrativeMapStopsXPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_narrative_map_stops_x_places_pkey");
            entity.ToTable("pacov_narrative_map_stops_x_places");
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.NarrativeMapStopId).HasColumnName("narrative_map_stop_id");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");
            
            entity.HasOne(d => d.NarrativeMapStop).WithMany(p => p.PacovNarrativeMapStopsXPlaces)
                .HasForeignKey(d => d.NarrativeMapStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_stop_id_fkey");

            entity.HasOne(d => d.Place).WithMany(p => p.PacovNarrativeMapStopsXPlaces)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.PacovNarrativeMapStopsXPlaces)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationship_type_fkey");
        });


        modelBuilder.Entity<PacovNarrativeMapXNarrativeMapStop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_narrative_maps_x_narrative_map_stops_pkey");
            entity.ToTable("pacov_narrative_maps_x_narrative_map_stops");
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.NarrativeMapId).HasColumnName("narrative_map_id");
            entity.Property(e => e.NarrativeMapStopId).HasColumnName("narrative_map_stop_id");

            entity.HasOne(d => d.NarrativeMap).WithMany(p => p.PacovNarrativeMapXNarrativeMapStops)
                .HasForeignKey(d => d.NarrativeMapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_id_fkey");

            entity.HasOne(d => d.NarrativeMapStop).WithMany(p => p.PacovNarrativeMapXNarrativeMapStops)
                .HasForeignKey(d => d.NarrativeMapStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_stop_id_fkey");
        });


        modelBuilder.Entity<PacovPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_places_pkey");

            entity.ToTable("pacov_places");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.HouseNr)
                .HasMaxLength(10)
                .HasColumnName("house_nr");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(250)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(250)
                .HasColumnName("label_en");
            entity.Property(e => e.RemarkCs).HasColumnName("remark_cs");
            entity.Property(e => e.RemarkEn).HasColumnName("remark_en");
            entity.Property(e => e.StreetCs)
                .HasMaxLength(100)
                .HasColumnName("street_cs");
            entity.Property(e => e.StreetEn)
                .HasMaxLength(100)
                .HasColumnName("street_en");
            entity.Property(e => e.TownCs)
                .HasMaxLength(100)
                .HasColumnName("town_cs");
            entity.Property(e => e.TownEn)
                .HasMaxLength(100)
                .HasColumnName("town_en");
            entity.Property(e => e.Type)
                .HasMaxLength(25)
                .HasColumnName("type");
        });

        modelBuilder.Entity<PacovPoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_pois_pkey");

            entity.ToTable("pacov_pois");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(250)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(250)
                .HasColumnName("label_en");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");

            entity.HasOne(d => d.Place).WithMany(p => p.PacovPois)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");
        });

        modelBuilder.Entity<PacovRelationshipType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_relationship_types_pkey");

            entity.ToTable("pacov_relationship_types");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(250)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(250)
                .HasColumnName("label_en");
            entity.Property(e => e.Table)
                .HasMaxLength(250)
                .HasColumnName("table");
        });

        modelBuilder.Entity<PacovTransport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pacov_transports_pkey");

            entity.ToTable("pacov_transports");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.PlaceFrom).HasColumnName("place_from");
            entity.Property(e => e.PlaceTo).HasColumnName("place_to");
            entity.Property(e => e.TransportCode)
                .HasMaxLength(10)
                .HasColumnName("transport_code");

            entity.HasOne(d => d.PlaceFromNavigation).WithMany(p => p.PacovTransportPlaceFromNavigations)
                .HasForeignKey(d => d.PlaceFrom)
                .HasConstraintName("place_from_fkey");

            entity.HasOne(d => d.PlaceToNavigation).WithMany(p => p.PacovTransportPlaceToNavigations)
                .HasForeignKey(d => d.PlaceTo)
                .HasConstraintName("place_to_fkey");
        });

        modelBuilder.Entity<PragueAddressesStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_addresses_stats_pkey");

            entity.ToTable("prague_addresses_stats");

            entity.HasIndex(e => e.Id, "id").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressCs).HasColumnName("address_cs");
            entity.Property(e => e.AddressCurrentCs).HasColumnName("address_current_cs");
            entity.Property(e => e.AddressCurrentDe).HasColumnName("address_current_de");
            entity.Property(e => e.AddressCurrentEn).HasColumnName("address_current_en");
            entity.Property(e => e.AddressDe).HasColumnName("address_de");
            entity.Property(e => e.AddressEn).HasColumnName("address_en");
            entity.Property(e => e.Count)
                .HasPrecision(5)
                .HasColumnName("count");
            entity.Property(e => e.FateUnknown)
                .HasPrecision(5)
                .HasColumnName("fate_unknown");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.Murdered)
                .HasPrecision(5)
                .HasColumnName("murdered");
            entity.Property(e => e.Present19420101)
                .HasPrecision(5)
                .HasColumnName("present_1942-01-01");
            entity.Property(e => e.Present19430101)
                .HasPrecision(5)
                .HasColumnName("present_1943-01-01");
            entity.Property(e => e.Present19440101)
                .HasPrecision(5)
                .HasColumnName("present_1944-01-01");
            entity.Property(e => e.Present19450101)
                .HasPrecision(5)
                .HasColumnName("present_1945-01-01");
            entity.Property(e => e.Present19450509)
                .HasPrecision(5)
                .HasColumnName("present_1945-05-09");
            entity.Property(e => e.Survived)
                .HasPrecision(5)
                .HasColumnName("survived");
        });

        modelBuilder.Entity<PragueAddressesStatsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_addresses_timeline_pkey");

            entity.ToTable("prague_addresses_stats_timeline");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressCs).HasColumnName("address_cs");
            entity.Property(e => e.AddressCurrentCs).HasColumnName("address_current_cs");
            entity.Property(e => e.AddressCurrentDe).HasColumnName("address_current_de");
            entity.Property(e => e.AddressCurrentEn).HasColumnName("address_current_en");
            entity.Property(e => e.AddressDe).HasColumnName("address_de");
            entity.Property(e => e.AddressEn).HasColumnName("address_en");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Deported19420101).HasColumnName("deported_1942-01-01");
            entity.Property(e => e.Deported19420401).HasColumnName("deported_1942-04-01");
            entity.Property(e => e.Deported19420701).HasColumnName("deported_1942-07-01");
            entity.Property(e => e.Deported19421001).HasColumnName("deported_1942-10-01");
            entity.Property(e => e.Deported19430101).HasColumnName("deported_1943-01-01");
            entity.Property(e => e.Deported19430401).HasColumnName("deported_1943-04-01");
            entity.Property(e => e.Deported19430701).HasColumnName("deported_1943-07-01");
            entity.Property(e => e.Deported19431001).HasColumnName("deported_1943-10-01");
            entity.Property(e => e.Deported19440101).HasColumnName("deported_1944-01-01");
            entity.Property(e => e.Deported19440401).HasColumnName("deported_1944-04-01");
            entity.Property(e => e.Deported19440701).HasColumnName("deported_1944-07-01");
            entity.Property(e => e.Deported19441001).HasColumnName("deported_1944-10-01");
            entity.Property(e => e.Deported19450101).HasColumnName("deported_1945-01-01");
            entity.Property(e => e.Deported19450401).HasColumnName("deported_1945-04-01");
            entity.Property(e => e.Deported19450508).HasColumnName("deported_1945-05-08");
            entity.Property(e => e.FateUnknown).HasColumnName("fate_unknown");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.Murdered).HasColumnName("murdered");
            entity.Property(e => e.Present19420101).HasColumnName("present_1942-01-01");
            entity.Property(e => e.Present19420101Perc).HasColumnName("present_1942-01-01_perc");
            entity.Property(e => e.Present19420401).HasColumnName("present_1942-04-01");
            entity.Property(e => e.Present19420401Perc).HasColumnName("present_1942-04-01_perc");
            entity.Property(e => e.Present19420701).HasColumnName("present_1942-07-01");
            entity.Property(e => e.Present19420701Perc).HasColumnName("present_1942-07-01_perc");
            entity.Property(e => e.Present19421001).HasColumnName("present_1942-10-01");
            entity.Property(e => e.Present19421001Perc).HasColumnName("present_1942-10-01_perc");
            entity.Property(e => e.Present19430101).HasColumnName("present_1943-01-01");
            entity.Property(e => e.Present19430101Perc).HasColumnName("present_1943-01-01_perc");
            entity.Property(e => e.Present19430401).HasColumnName("present_1943-04-01");
            entity.Property(e => e.Present19430401Perc).HasColumnName("present_1943-04-01_perc");
            entity.Property(e => e.Present19430701).HasColumnName("present_1943-07-01");
            entity.Property(e => e.Present19430701Perc).HasColumnName("present_1943-07-01_perc");
            entity.Property(e => e.Present19431001).HasColumnName("present_1943-10-01");
            entity.Property(e => e.Present19431001Perc).HasColumnName("present_1943-10-01_perc");
            entity.Property(e => e.Present19440101).HasColumnName("present_1944-01-01");
            entity.Property(e => e.Present19440101Perc).HasColumnName("present_1944-01-01_perc");
            entity.Property(e => e.Present19440401).HasColumnName("present_1944-04-01");
            entity.Property(e => e.Present19440401Perc).HasColumnName("present_1944-04-01_perc");
            entity.Property(e => e.Present19440701).HasColumnName("present_1944-07-01");
            entity.Property(e => e.Present19440701Perc).HasColumnName("present_1944-07-01_perc");
            entity.Property(e => e.Present19441001).HasColumnName("present_1944-10-01");
            entity.Property(e => e.Present19441001Perc).HasColumnName("present_1944-10-01_perc");
            entity.Property(e => e.Present19450101).HasColumnName("present_1945-01-01");
            entity.Property(e => e.Present19450101Perc).HasColumnName("present_1945-01-01_perc");
            entity.Property(e => e.Present19450401).HasColumnName("present_1945-04-01");
            entity.Property(e => e.Present19450401Perc).HasColumnName("present_1945-04-01_perc");
            entity.Property(e => e.Present19450508).HasColumnName("present_1945-05-08");
            entity.Property(e => e.Present19450508Perc).HasColumnName("present_1945-05-08_perc");
            entity.Property(e => e.Survived).HasColumnName("survived");
        });

        modelBuilder.Entity<PragueIncident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_pkey");

            entity.ToTable("prague_incidents");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.DocumentsCs).HasColumnName("documents_cs");
            entity.Property(e => e.DocumentsEn).HasColumnName("documents_en");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.LabelCs).HasColumnName("label_cs");
            entity.Property(e => e.LabelEn).HasColumnName("label_en");
            entity.Property(e => e.PlaceCs).HasColumnName("place_cs");
            entity.Property(e => e.PlaceEn).HasColumnName("place_en");
            entity.Property(e => e.Spec1Cs).HasColumnName("spec_1_cs");
            entity.Property(e => e.Spec1En).HasColumnName("spec_1_en");
            entity.Property(e => e.Spec2Cs).HasColumnName("spec_2_cs");
            entity.Property(e => e.Spec2En).HasColumnName("spec_2_en");
            entity.Property(e => e.Type1Cs).HasColumnName("type_1_cs");
            entity.Property(e => e.Type1En).HasColumnName("type_1_en");
            entity.Property(e => e.Type2Cs).HasColumnName("type_2_cs");
            entity.Property(e => e.Type2En).HasColumnName("type_2_en");
        });

        modelBuilder.Entity<PragueIncidentsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_timeline_pkey");

            entity.ToTable("prague_incidents_timeline");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DateIso).HasColumnName("date_iso");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.DocumentsCs).HasColumnName("documents_cs");
            entity.Property(e => e.DocumentsEn).HasColumnName("documents_en");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.LabelCs).HasColumnName("label_cs");
            entity.Property(e => e.LabelEn).HasColumnName("label_en");
            entity.Property(e => e.PlaceCs).HasColumnName("place_cs");
            entity.Property(e => e.PlaceEn).HasColumnName("place_en");
            entity.Property(e => e.Spec1Cs).HasColumnName("spec_1_cs");
            entity.Property(e => e.Spec1En).HasColumnName("spec_1_en");
            entity.Property(e => e.Spec2Cs).HasColumnName("spec_2_cs");
            entity.Property(e => e.Spec2En).HasColumnName("spec_2_en");
            entity.Property(e => e.Type1Cs).HasColumnName("type_1_cs");
            entity.Property(e => e.Type1En).HasColumnName("type_1_en");
            entity.Property(e => e.Type2Cs).HasColumnName("type_2_cs");
            entity.Property(e => e.Type2En).HasColumnName("type_2_en");
        });

        modelBuilder.Entity<PragueIncidentsXDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_x_documents_pkey");

            entity.ToTable("prague_incidents_x_documents");

            entity.HasIndex(e => e.IncidentId, "fki_prague_incidents_x_documents_incident_id_ref_prague_inciden");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentCs).HasColumnName("document_cs");
            entity.Property(e => e.DocumentEn).HasColumnName("document_en");
            entity.Property(e => e.Img).HasColumnName("img");
            entity.Property(e => e.IncidentId).HasColumnName("incident_id");
        });


        modelBuilder.Entity<PragueLastResidence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_last_residence_pkey");

            entity.ToTable("prague_last_residence");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.VictimId).HasColumnName("victim_id");

            entity.HasOne(d => d.Address).WithMany(p => p.PragueLastResidences)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("address_id_fkey");

            entity.HasOne(d => d.Victim).WithMany(p => p.PragueLastResidences)
                .HasForeignKey(d => d.VictimId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("victim_id_fkey");

        });

        modelBuilder.Entity<PraguePlacesOfInterest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_places_of_interest_pkey");

            entity.ToTable("prague_places_of_interest");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressCs).HasColumnName("address_cs");
            entity.Property(e => e.AddressEn).HasColumnName("address_en");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.GeographyPolygon).HasColumnName("geography_polygon");
            entity.Property(e => e.Inaccessible).HasColumnName("inaccessible");
            entity.Property(e => e.LabelCs).HasColumnName("label_cs");
            entity.Property(e => e.LabelEn).HasColumnName("label_en");
            entity.Property(e => e.Quarter1938).HasColumnName("quarter_1938");
        });

        modelBuilder.Entity<PraguePlacesOfInterestTimeline>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("prague_places_of_interest_timeline");

            entity.Property(e => e.AddressCs).HasColumnName("address_cs");
            entity.Property(e => e.AddressEn).HasColumnName("address_en");
            entity.Property(e => e.DateFrom).HasColumnName("date_from");
            entity.Property(e => e.DateTo).HasColumnName("date_to");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.GeographyPolygon).HasColumnName("geography_polygon");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Inaccessible).HasColumnName("inaccessible");
            entity.Property(e => e.LabelCs).HasColumnName("label_cs");
            entity.Property(e => e.LabelEn).HasColumnName("label_en");
            entity.Property(e => e.Quarter1938).HasColumnName("quarter_1938");
        });

        modelBuilder.Entity<PraguePlacesOfMemory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_places_of_memory_pkey");

            entity.ToTable("prague_places_of_memory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressCs).HasColumnName("address_cs");
            entity.Property(e => e.AddressEn).HasColumnName("address_en");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.Label).HasColumnName("label");
            entity.Property(e => e.LinkHolocaustCs).HasColumnName("link_holocaust_cs");
            entity.Property(e => e.LinkHolocaustEn).HasColumnName("link_holocaust_en");
            entity.Property(e => e.LinkStolpersteineCs).HasColumnName("link_stolpersteine_cs");
            entity.Property(e => e.LinkStolpersteineEn).HasColumnName("link_stolpersteine_en");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<PragueQuartersStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_quarters_stats_pkey");

            entity.ToTable("prague_quarters_stats");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.GeographyPolygon).HasColumnName("geography_polygon");
            entity.Property(e => e.QuarterCs).HasColumnName("quarter_cs");
            entity.Property(e => e.QuarterEn).HasColumnName("quarter_en");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<PragueQuartersStatsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_quarters_stats_timeline_pkey");

            entity.ToTable("prague_quarters_stats_timeline");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasColumnName("geography");
            entity.Property(e => e.GeographyPolygon).HasColumnName("geography_polygon");
            entity.Property(e => e.QuarterCs).HasColumnName("quarter_cs");
            entity.Property(e => e.QuarterEn).HasColumnName("quarter_en");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e._19420101).HasColumnName("1942-01-01");
            entity.Property(e => e._19420401).HasColumnName("1942-04-01");
            entity.Property(e => e._19420701).HasColumnName("1942-07-01");
            entity.Property(e => e._19421001).HasColumnName("1942-10-01");
            entity.Property(e => e._19430101).HasColumnName("1943-01-01");
            entity.Property(e => e._19430401).HasColumnName("1943-04-01");
            entity.Property(e => e._19430701).HasColumnName("1943-07-01");
            entity.Property(e => e._19431001).HasColumnName("1943-10-01");
            entity.Property(e => e._19440101).HasColumnName("1944-01-01");
            entity.Property(e => e._19440401).HasColumnName("1944-04-01");
            entity.Property(e => e._19440701).HasColumnName("1944-07-01");
            entity.Property(e => e._19441001).HasColumnName("1944-10-01");
            entity.Property(e => e._19450101).HasColumnName("1945-01-01");
            entity.Property(e => e._19450401).HasColumnName("1945-04-01");
            entity.Property(e => e._19450508).HasColumnName("1945-05-08");
        });

        modelBuilder.Entity<PragueVictim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_victims_pkey");

            entity.ToTable("prague_victims");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressCs).HasColumnName("address_cs");
            entity.Property(e => e.AddressEn).HasColumnName("address_en");
            entity.Property(e => e.DetailsCs).HasColumnName("details_cs");
            entity.Property(e => e.DetailsEn).HasColumnName("details_en");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.Label).HasColumnName("label");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");

            entity.HasOne(d => d.Place).WithMany(p => p.PragueVictims)
                .HasForeignKey(d => d.PlaceId)
                .HasConstraintName("prague_victims_fkey");
        });

        modelBuilder.Entity<PragueVictimsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_victims_timeline_pkey");

            entity.ToTable("prague_victims_timeline");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressCs).HasColumnName("address_cs");
            entity.Property(e => e.AddressEn).HasColumnName("address_en");
            entity.Property(e => e.DetailsCs).HasColumnName("details_cs");
            entity.Property(e => e.DetailsEn).HasColumnName("details_en");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.Label).HasColumnName("label");
            entity.Property(e => e.Photo).HasColumnName("photo");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.TransportDate).HasColumnName("transport_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
