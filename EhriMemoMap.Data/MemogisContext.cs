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

    public virtual DbSet<MapObject> MapObjects { get; set; }

    public virtual DbSet<MapStatistic> MapStatistics { get; set; }

    public virtual DbSet<PragueAddressesStat> PragueAddressesStats { get; set; }

    public virtual DbSet<PragueAddressesStatsTimeline> PragueAddressesStatsTimelines { get; set; }

    public virtual DbSet<PragueIncident> PragueIncidents { get; set; }

    public virtual DbSet<PragueIncidentsTimeline> PragueIncidentsTimelines { get; set; }

    public virtual DbSet<PragueIncidentsXDocument> PragueIncidentsXDocuments { get; set; }

    public virtual DbSet<PraguePlacesOfInterest> PraguePlacesOfInterests { get; set; }

    public virtual DbSet<PraguePlacesOfInterestTimeline> PraguePlacesOfInterestTimelines { get; set; }

    public virtual DbSet<PragueQuartersStat> PragueQuartersStats { get; set; }

    public virtual DbSet<PragueQuartersStatsTimeline> PragueQuartersStatsTimelines { get; set; }

    public virtual DbSet<PraguePlacesOfMemory> PraguePlacesOfMemories { get; set; }

    public virtual DbSet<PragueVictim> PragueVictims { get; set; }

    public virtual DbSet<PragueVictimsTimeline> PragueVictimsTimelines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(x => x
                .UseNodaTime()
                .UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<MapObject>(entity =>
        {
            entity.ToView("map_objects");
        });

        modelBuilder.Entity<MapStatistic>(entity =>
        {
            entity.ToView("map_statistics");
        });

        modelBuilder.Entity<PragueAddressesStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_addresses_stats_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PragueAddressesStatsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_addresses_timeline_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PragueIncident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PragueIncidentsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_timeline_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PragueIncidentsXDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_incidents_x_documents_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PraguePlacesOfInterest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_places_of_interest_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PragueQuartersStat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_quarters_stats_pkey");
        });

        modelBuilder.Entity<PragueQuartersStatsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_quarters_stats_timeline_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PraguePlacesOfMemory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_places_of_memory_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<PragueVictim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_victims_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Place).WithMany(p => p.PragueVictims).HasConstraintName("prague_victims_fkey");
        });

        modelBuilder.Entity<PragueVictimsTimeline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prague_victims_timeline_pkey");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
