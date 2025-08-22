using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EhriMemoMap.Data.Ricany;

public partial class RicanyContext : DbContext
{
    public RicanyContext()
    {
    }

    public RicanyContext(DbContextOptions<RicanyContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql(x => x
            .UseNodaTime()
            .UseNetTopologySuite());

    public virtual DbSet<MapObject> MapObjects { get; set; }

    public virtual DbSet<MapStatistic> MapStatistics { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentsXEntity> DocumentsXEntities { get; set; }

    public virtual DbSet<DocumentsXEvent> DocumentsXEvents { get; set; }

    public virtual DbSet<DocumentsXMedium> DocumentsXMedia { get; set; }

    public virtual DbSet<DocumentsXNarrativeMapStop> DocumentsXNarrativeMapStops { get; set; }

    public virtual DbSet<DocumentsXPlacesOfMemory> DocumentsXPlacesOfMemories { get; set; }

    public virtual DbSet<DocumentsXPoi> DocumentsXPois { get; set; }

    public virtual DbSet<EntitiesXEntity> EntitiesXEntities { get; set; }

    public virtual DbSet<EntitiesXMedium> EntitiesXMedia { get; set; }

    public virtual DbSet<EntitiesXNarrativeMap> EntitiesXNarrativeMaps { get; set; }

    public virtual DbSet<EntitiesXPlace> EntitiesXPlaces { get; set; }

    public virtual DbSet<EntitiesXTransport> EntitiesXTransports { get; set; }

    public virtual DbSet<Entity> Entities { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventsXPlace> EventsXPlaces { get; set; }

    public virtual DbSet<Incident> Incidents { get; set; }

    public virtual DbSet<ListItem> ListItems { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<NarrativeMap> NarrativeMaps { get; set; }

    public virtual DbSet<NarrativeMapStop> NarrativeMapStops { get; set; }

    public virtual DbSet<NarrativeMapStopsXPlace> NarrativeMapStopsXPlaces { get; set; }

    public virtual DbSet<NarrativeMapsXNarrativeMapStop> NarrativeMapsXNarrativeMapStops { get; set; }

    public virtual DbSet<Place> Places { get; set; }

    public virtual DbSet<PlacesOfMemory> PlacesOfMemories { get; set; }

    public virtual DbSet<PlacesOfMemoryXPlacesOfMemory> PlacesOfMemoryXPlacesOfMemories { get; set; }

    public virtual DbSet<PlacesXPlacesOfMemory> PlacesXPlacesOfMemories { get; set; }

    public virtual DbSet<Poi> Pois { get; set; }

    public virtual DbSet<RelationshipType> RelationshipTypes { get; set; }

    public virtual DbSet<Transport> Transports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("dblink")
            .HasPostgresExtension("postgis");

        modelBuilder.Entity<MapObject>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("map_objects");

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

        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_pkey");

            entity.ToTable("documents");

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

            entity.HasOne(d => d.CreationPlaceNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CreationPlace)
                .HasConstraintName("creation_place_fkey");
        });

        modelBuilder.Entity<DocumentsXEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_x_entities_pkey");

            entity.ToTable("documents_x_entities");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentsXEntities)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.Entity).WithMany(p => p.DocumentsXEntities)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");
        });

        modelBuilder.Entity<DocumentsXEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_x_events_pkey");

            entity.ToTable("documents_x_events");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentsXEvents)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.Event).WithMany(p => p.DocumentsXEvents)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("event_id_fkey");
        });

        modelBuilder.Entity<DocumentsXMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_x_media_pkey");

            entity.ToTable("documents_x_media");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.MediaId).HasColumnName("media_id");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentsXMedia)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.Media).WithMany(p => p.DocumentsXMedia)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("media_id_fkey");
        });

        modelBuilder.Entity<DocumentsXNarrativeMapStop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_x_narrative_map_stops_pkey");

            entity.ToTable("documents_x_narrative_map_stops");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.NarrativeMapStopId).HasColumnName("narrative_map_stop_id");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentsXNarrativeMapStops)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.NarrativeMapStop).WithMany(p => p.DocumentsXNarrativeMapStops)
                .HasForeignKey(d => d.NarrativeMapStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_stop_id_fkey");
        });

        modelBuilder.Entity<DocumentsXPlacesOfMemory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_x_places_of_memory_pkey");

            entity.ToTable("documents_x_places_of_memory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.PlaceOfMemoryId).HasColumnName("place_of_memory_id");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentsXPlacesOfMemories)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.PlaceOfMemory).WithMany(p => p.DocumentsXPlacesOfMemories)
                .HasForeignKey(d => d.PlaceOfMemoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_of_memory_id_fkey");
        });

        modelBuilder.Entity<DocumentsXPoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_x_pois_pkey");

            entity.ToTable("documents_x_pois");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentsXPois)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_id_fkey");

            entity.HasOne(d => d.Poi).WithMany(p => p.DocumentsXPois)
                .HasForeignKey(d => d.PoiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("poi_id_fkey");
        });

        modelBuilder.Entity<EntitiesXEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("entities_x_entities_pkey");

            entity.ToTable("entities_x_entities");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Entity1Id).HasColumnName("entity_1_id");
            entity.Property(e => e.Entity2Id).HasColumnName("entity_2_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.Entity1).WithMany(p => p.EntitiesXEntityEntity1s)
                .HasForeignKey(d => d.Entity1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_1_id_fkey");

            entity.HasOne(d => d.Entity2).WithMany(p => p.EntitiesXEntityEntity2s)
                .HasForeignKey(d => d.Entity2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_2_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.EntitiesXEntities)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationship_type_fkey");
        });

        modelBuilder.Entity<EntitiesXMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("entities_x_media_pkey");

            entity.ToTable("entities_x_media");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.MediaId).HasColumnName("media_id");

            entity.HasOne(d => d.Entity).WithMany(p => p.EntitiesXMedia)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.Media).WithMany(p => p.EntitiesXMedia)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("media_id_fkey");
        });

        modelBuilder.Entity<EntitiesXNarrativeMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("entities_x_narrative_maps_pkey");

            entity.ToTable("entities_x_narrative_maps");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.NarrativeMapId).HasColumnName("narrative_map_id");

            entity.HasOne(d => d.Entity).WithMany(p => p.EntitiesXNarrativeMaps)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.NarrativeMap).WithMany(p => p.EntitiesXNarrativeMaps)
                .HasForeignKey(d => d.NarrativeMapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_id_fkey");
        });

        modelBuilder.Entity<EntitiesXPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("entities_x_places_pkey");

            entity.ToTable("entities_x_places");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DateFrom).HasColumnName("date_from");
            entity.Property(e => e.DateTo).HasColumnName("date_to");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.Entity).WithMany(p => p.EntitiesXPlaces)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.Place).WithMany(p => p.EntitiesXPlaces)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.EntitiesXPlaces)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationsip_type_fkey");
        });

        modelBuilder.Entity<EntitiesXTransport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("entities_x_transports_pkey");

            entity.ToTable("entities_x_transports");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.NrInTransport).HasColumnName("nr_in_transport");
            entity.Property(e => e.TransportId).HasColumnName("transport_id");
            entity.Property(e => e.TransportOrder).HasColumnName("transport_order");

            entity.HasOne(d => d.Entity).WithMany(p => p.EntitiesXTransports)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entity_id_fkey");

            entity.HasOne(d => d.Transport).WithMany(p => p.EntitiesXTransports)
                .HasForeignKey(d => d.TransportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transport_id_fkey");
        });

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("entities_pkey");

            entity.ToTable("entities");

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

            entity.HasOne(d => d.FateNavigation).WithMany(p => p.EntityFateNavigations)
                .HasForeignKey(d => d.Fate)
                .HasConstraintName("fate_fkey");

            entity.HasOne(d => d.SexNavigation).WithMany(p => p.EntitySexNavigations)
                .HasForeignKey(d => d.Sex)
                .HasConstraintName("sex_fkey");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("events_pkey");

            entity.ToTable("events");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.LabelCs).HasColumnName("label_cs");
            entity.Property(e => e.LabelEn).HasColumnName("label_en");
            entity.Property(e => e.LinkCs).HasColumnName("link_cs");
            entity.Property(e => e.LinkEn).HasColumnName("link_en");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<EventsXPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("events_x_places_pkey");

            entity.ToTable("events_x_places");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.Event).WithMany(p => p.EventsXPlaces)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("event_id_fkey");

            entity.HasOne(d => d.Place).WithMany(p => p.EventsXPlaces)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.EventsXPlaces)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationship_type_fkey");
        });

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("incidents_pkey");

            entity.ToTable("incidents");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DateCs)
                .HasMaxLength(100)
                .HasColumnName("date_cs");
            entity.Property(e => e.DateEn)
                .HasMaxLength(100)
                .HasColumnName("date_en");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.LabelCs)
                .HasMaxLength(250)
                .HasColumnName("label_cs");
            entity.Property(e => e.LabelEn)
                .HasMaxLength(250)
                .HasColumnName("label_en");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");

            entity.HasOne(d => d.Place).WithMany(p => p.Incidents)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");
        });

        modelBuilder.Entity<ListItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("list_items_pkey");

            entity.ToTable("list_items");

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

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_pkey");

            entity.ToTable("media");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.OmekaId).HasColumnName("omeka_id");
            entity.Property(e => e.OmekaUrl)
                .HasMaxLength(250)
                .HasColumnName("omeka_url");
            entity.Property(e => e.OmekaThumbnailUrl)
                .HasMaxLength(250)
                .HasColumnName("omeka_thumbnail_url");
        });

        modelBuilder.Entity<NarrativeMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("narrative_map_pkey");

            entity.ToTable("narrative_maps");

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
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.NarrativeMaps)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("type_fkey");
        });

        modelBuilder.Entity<NarrativeMapStop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("narrative_map_stops_pkey");

            entity.ToTable("narrative_map_stops");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DateCs)
                .HasMaxLength(250)
                .HasColumnName("date_cs");
            entity.Property(e => e.DateEn)
                .HasMaxLength(250)
                .HasColumnName("date_en");
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

        modelBuilder.Entity<NarrativeMapStopsXPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("narrative_map_stops_x_places_pkey");

            entity.ToTable("narrative_map_stops_x_places");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.NarrativeMapStopId).HasColumnName("narrative_map_stop_id");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.NarrativeMapStop).WithMany(p => p.NarrativeMapStopsXPlaces)
                .HasForeignKey(d => d.NarrativeMapStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_stop_fkey");

            entity.HasOne(d => d.Place).WithMany(p => p.NarrativeMapStopsXPlaces)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.NarrativeMapStopsXPlaces)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationship_type_fkey");
        });

        modelBuilder.Entity<NarrativeMapsXNarrativeMapStop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("narrative_maps_x_narrative_map_stops_pkey");

            entity.ToTable("narrative_maps_x_narrative_map_stops");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.NarrativeMapId).HasColumnName("narrative_map_id");
            entity.Property(e => e.NarrativeMapStopId).HasColumnName("narrative_map_stop_id");

            entity.HasOne(d => d.NarrativeMap).WithMany(p => p.NarrativeMapsXNarrativeMapStops)
                .HasForeignKey(d => d.NarrativeMapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_fkey");

            entity.HasOne(d => d.NarrativeMapStop).WithMany(p => p.NarrativeMapsXNarrativeMapStops)
                .HasForeignKey(d => d.NarrativeMapStopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("narrative_map_stop_fkey");
        });

        modelBuilder.Entity<Place>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("places_pkey");

            entity.ToTable("places");

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

        modelBuilder.Entity<PlacesOfMemory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("places_of_memory_pkey");

            entity.ToTable("places_of_memory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreationDate).HasColumnName("creation_date");
            entity.Property(e => e.DescriptionCs).HasColumnName("description_cs");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.InscriptionCs).HasColumnName("inscription_cs");
            entity.Property(e => e.InscriptionEn).HasColumnName("inscription_en");
            entity.Property(e => e.LabelCs).HasColumnName("label_cs");
            entity.Property(e => e.LabelEn).HasColumnName("label_en");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<PlacesOfMemoryXPlacesOfMemory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("places_of_memory_x_places_of_memory_pkey");

            entity.ToTable("places_of_memory_x_places_of_memory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.PlaceOfMemory1Id).HasColumnName("place_of_memory_1_id");
            entity.Property(e => e.PlaceOfMemory2Id).HasColumnName("place_of_memory_2_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.PlaceOfMemory1).WithMany(p => p.PlacesOfMemoryXPlacesOfMemoryPlaceOfMemory1s)
                .HasForeignKey(d => d.PlaceOfMemory1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_of_memory_1_id_fkey");

            entity.HasOne(d => d.PlaceOfMemory2).WithMany(p => p.PlacesOfMemoryXPlacesOfMemoryPlaceOfMemory2s)
                .HasForeignKey(d => d.PlaceOfMemory2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_of_memory_2_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.PlacesOfMemoryXPlacesOfMemories)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationship_type_fkey");
        });

        modelBuilder.Entity<PlacesXPlacesOfMemory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("places_x_places_of_memory_pkey");

            entity.ToTable("places_x_places_of_memory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.PlaceId).HasColumnName("place_id");
            entity.Property(e => e.PlaceOfMemoryId).HasColumnName("place_of_memory_id");
            entity.Property(e => e.RelationshipType).HasColumnName("relationship_type");

            entity.HasOne(d => d.Place).WithMany(p => p.PlacesXPlacesOfMemories)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");

            entity.HasOne(d => d.PlaceOfMemory).WithMany(p => p.PlacesXPlacesOfMemories)
                .HasForeignKey(d => d.PlaceOfMemoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_of_memory_id_fkey");

            entity.HasOne(d => d.RelationshipTypeNavigation).WithMany(p => p.PlacesXPlacesOfMemories)
                .HasForeignKey(d => d.RelationshipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relationship_type_fkey");
        });

        modelBuilder.Entity<Poi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pois_pkey");

            entity.ToTable("pois");

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

            entity.HasOne(d => d.Place).WithMany(p => p.Pois)
                .HasForeignKey(d => d.PlaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("place_id_fkey");
        });

        modelBuilder.Entity<RelationshipType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("relationship_types_pkey");

            entity.ToTable("relationship_types");

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

        modelBuilder.Entity<Transport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transports_pkey");

            entity.ToTable("transports");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.PlaceFrom).HasColumnName("place_from");
            entity.Property(e => e.PlaceTo).HasColumnName("place_to");
            entity.Property(e => e.TransportCode)
                .HasMaxLength(10)
                .HasColumnName("transport_code");

            entity.HasOne(d => d.PlaceFromNavigation).WithMany(p => p.TransportPlaceFromNavigations)
                .HasForeignKey(d => d.PlaceFrom)
                .HasConstraintName("place_from_fkey");

            entity.HasOne(d => d.PlaceToNavigation).WithMany(p => p.TransportPlaceToNavigations)
                .HasForeignKey(d => d.PlaceTo)
                .HasConstraintName("place_to_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
