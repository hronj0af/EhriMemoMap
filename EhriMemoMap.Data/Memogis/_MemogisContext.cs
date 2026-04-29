using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EhriMemoMap.Data.Memogis;

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





    
    public virtual DbSet<AddressesStat> AddressesStats { get; set; }

    public virtual DbSet<AddressesStatsTimeline> AddressesStatsTimelines { get; set; }

    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<DocumentsXMedium> DocumentsXMedia { get; set; }
    public virtual DbSet<DocumentsXEntity> DocumentsXEntities { get; set; }

    public virtual DbSet<EntitiesNoTimeline> EntitiesNoTimelines { get; set; }

    public virtual DbSet<Entity> Entities { get; set; }

    public virtual DbSet<EntitiesXMedium> EntitiesXMedia { get; set; }
    public virtual DbSet<Incident> Incidents { get; set; }

    public virtual DbSet<IncidentsTimeline> IncidentsTimelines { get; set; }

    public virtual DbSet<IncidentsXDocument> IncidentsXDocuments { get; set; }

    public virtual DbSet<LastResidence> LastResidences { get; set; }

    public virtual DbSet<MapObject> MapObjects { get; set; }

    public virtual DbSet<MapStatistic> MapStatistics { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Place> Places { get; set; }

    public virtual DbSet<PlacesOfInterest> PlacesOfInterests { get; set; }

    public virtual DbSet<PlacesOfInterestTimeline> PlacesOfInterestTimelines { get; set; }

    public virtual DbSet<PlacesOfMemory> PlacesOfMemories { get; set; }

    public virtual DbSet<QuartersStat> QuartersStats { get; set; }

    public virtual DbSet<QuartersStatsTimeline> QuartersStatsTimelines { get; set; }



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


        modelBuilder.Entity<AddressesStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_addresses_stats_pkey");

            entity.ToTable("addresses_stats");

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

        modelBuilder.Entity<AddressesStatsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_addresses_timeline_pkey");

            entity.ToTable("addresses_stats_timeline");

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
            entity.Property(e => e.OwnerCs)
                .HasMaxLength(250)
                .HasColumnName("owner_cs");
            entity.Property(e => e.OwnerEn)
                .HasMaxLength(250)
                .HasColumnName("owner_en");
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

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_pkey");

            entity.ToTable("incidents");

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

        modelBuilder.Entity<IncidentsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_timeline_pkey");

            entity.ToTable("incidents_timeline");

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

        modelBuilder.Entity<IncidentsXDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_x_documents_pkey");

            entity.ToTable("incidents_x_documents");

            entity.HasIndex(e => e.IncidentId, "fki_prague_incidents_x_documents_incident_id_ref_prague_inciden");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DocumentCs).HasColumnName("document_cs");
            entity.Property(e => e.DocumentEn).HasColumnName("document_en");
            entity.Property(e => e.Img).HasColumnName("img");
            entity.Property(e => e.IncidentId).HasColumnName("incident_id");
        });


        modelBuilder.Entity<LastResidence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_last_residence_pkey");

            entity.ToTable("last_residence");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.VictimId).HasColumnName("victim_id");

            entity.HasOne(d => d.Address).WithMany(p => p.PragueLastResidences)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("address_id_fkey");

            entity.HasOne(d => d.Victim).WithMany(p => p.LastResidences)
                .HasForeignKey(d => d.VictimId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("victim_id_fkey");

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

        modelBuilder.Entity<PlacesOfInterest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_places_of_interest_pkey");

            entity.ToTable("places_of_interest");

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

        modelBuilder.Entity<PlacesOfInterestTimeline>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("places_of_interest_timeline");

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

        modelBuilder.Entity<PlacesOfMemory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_places_of_memory_pkey");

            entity.ToTable("places_of_memory");

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

        modelBuilder.Entity<QuartersStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_quarters_stats_pkey");

            entity.ToTable("quarters_stats");

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

        modelBuilder.Entity<QuartersStatsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_quarters_stats_timeline_pkey");

            entity.ToTable("quarters_stats_timeline");

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

        modelBuilder.Entity<EntitiesNoTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_victims_pkey");

            entity.ToTable("entities_no_timeline");

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

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_victims_timeline_pkey");

            entity.ToTable("entities");

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
