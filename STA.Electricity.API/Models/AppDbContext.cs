using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace STA.Electricity.API.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<Cabin> Cabins { get; set; }

    public virtual DbSet<Cable> Cables { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<CuttingDownA> CuttingDownAs { get; set; }

    public virtual DbSet<CuttingDownB> CuttingDownBs { get; set; }

    public virtual DbSet<CuttingDownDetail> CuttingDownDetails { get; set; }

    public virtual DbSet<CuttingDownHeader> CuttingDownHeaders { get; set; }

    public virtual DbSet<CuttingDownIgnored> CuttingDownIgnoreds { get; set; }

    public virtual DbSet<Flat> Flats { get; set; }

    public virtual DbSet<Governrate> Governrates { get; set; }

    public virtual DbSet<NetworkElement> NetworkElements { get; set; }

    public virtual DbSet<NetworkElementHierarchyPath> NetworkElementHierarchyPaths { get; set; }

    public virtual DbSet<NetworkElementType> NetworkElementTypes { get; set; }

    public virtual DbSet<ProblemType> ProblemTypes { get; set; }

    public virtual DbSet<ProblemType1> ProblemTypes1 { get; set; }

    public virtual DbSet<Sector> Sectors { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Tower> Towers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Zone> Zones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=ElectricityOutageDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.BlockKey).HasName("PK__Block__DF1963F13F4A0508");

            entity.ToTable("Block", "STA");

            entity.HasIndex(e => e.CableKey, "IX_STA_Block_Cable_Key");

            entity.Property(e => e.BlockKey)
                .ValueGeneratedNever()
                .HasColumnName("Block_Key");
            entity.Property(e => e.BlockName)
                .HasMaxLength(100)
                .HasColumnName("Block_Name");
            entity.Property(e => e.CableKey).HasColumnName("Cable_Key");

            entity.HasOne(d => d.CableKeyNavigation).WithMany(p => p.Blocks)
                .HasForeignKey(d => d.CableKey)
                .HasConstraintName("FK_Block_Cable");
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.BuildingKey).HasName("PK__Building__0330590134648AA7");

            entity.ToTable("Building", "STA");

            entity.HasIndex(e => e.BlockKey, "IX_STA_Building_Block_Key");

            entity.Property(e => e.BuildingKey)
                .ValueGeneratedNever()
                .HasColumnName("Building_Key");
            entity.Property(e => e.BlockKey).HasColumnName("Block_Key");
            entity.Property(e => e.BuildingName)
                .HasMaxLength(100)
                .HasColumnName("Building_Name");

            entity.HasOne(d => d.BlockKeyNavigation).WithMany(p => p.Buildings)
                .HasForeignKey(d => d.BlockKey)
                .HasConstraintName("FK_Building_Block");
        });

        modelBuilder.Entity<Cabin>(entity =>
        {
            entity.HasKey(e => e.CabinKey).HasName("PK__Cabin__341FCAA0FADD01AE");

            entity.ToTable("Cabin", "STA");

            entity.HasIndex(e => e.TowerKey, "IX_STA_Cabin_Tower_Key");

            entity.Property(e => e.CabinKey)
                .ValueGeneratedNever()
                .HasColumnName("Cabin_Key");
            entity.Property(e => e.CabinName)
                .HasMaxLength(100)
                .HasColumnName("Cabin_Name");
            entity.Property(e => e.TowerKey).HasColumnName("Tower_Key");

            entity.HasOne(d => d.TowerKeyNavigation).WithMany(p => p.Cabins)
                .HasForeignKey(d => d.TowerKey)
                .HasConstraintName("FK_Cabin_Tower");
        });

        modelBuilder.Entity<Cable>(entity =>
        {
            entity.HasKey(e => e.CableKey).HasName("PK__Cable__5420959C78C6EB7F");

            entity.ToTable("Cable", "STA");

            entity.HasIndex(e => e.CabinKey, "IX_STA_Cable_Cabin_Key");

            entity.Property(e => e.CableKey)
                .ValueGeneratedNever()
                .HasColumnName("Cable_Key");
            entity.Property(e => e.CabinKey).HasColumnName("Cabin_Key");
            entity.Property(e => e.CableName)
                .HasMaxLength(100)
                .HasColumnName("Cable_Name");

            entity.HasOne(d => d.CabinKeyNavigation).WithMany(p => p.Cables)
                .HasForeignKey(d => d.CabinKey)
                .HasConstraintName("FK_Cable_Cabin");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.ChannelKey).HasName("PK__Channel__011B50F4CA443857");

            entity.ToTable("Channel", "FTA");

            entity.Property(e => e.ChannelKey)
                .ValueGeneratedNever()
                .HasColumnName("Channel_Key");
            entity.Property(e => e.ChannelName)
                .HasMaxLength(100)
                .HasColumnName("Channel_Name");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityKey).HasName("PK__City__2D42E441655AEBDB");

            entity.ToTable("City", "STA");

            entity.HasIndex(e => e.ZoneKey, "IX_STA_City_Zone_Key");

            entity.Property(e => e.CityKey)
                .ValueGeneratedNever()
                .HasColumnName("City_Key");
            entity.Property(e => e.CityName)
                .HasMaxLength(100)
                .HasColumnName("City_Name");
            entity.Property(e => e.ZoneKey).HasColumnName("Zone_Key");

            entity.HasOne(d => d.ZoneKeyNavigation).WithMany(p => p.Cities)
                .HasForeignKey(d => d.ZoneKey)
                .HasConstraintName("FK_City_Zone");
        });

        modelBuilder.Entity<CuttingDownA>(entity =>
        {
            entity.HasKey(e => e.CuttingDownAIncidentId).HasName("PK__Cutting___DB73CADECB8609B9");

            entity.ToTable("Cutting_Down_A", "STA");

            entity.Property(e => e.CuttingDownAIncidentId)
                .ValueGeneratedNever()
                .HasColumnName("Cutting_Down_A_Incident_ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser).HasMaxLength(100);
            entity.Property(e => e.CuttingDownCabinName)
                .HasMaxLength(100)
                .HasColumnName("Cutting_Down_Cabin_Name");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.PlannedEndDts)
                .HasColumnType("datetime")
                .HasColumnName("PlannedEndDTS");
            entity.Property(e => e.PlannedStartDts)
                .HasColumnType("datetime")
                .HasColumnName("PlannedStartDTS");
            entity.Property(e => e.ProblemTypeKey).HasColumnName("Problem_Type_Key");
            entity.Property(e => e.UpdatedUser).HasMaxLength(100);

            entity.HasOne(d => d.ProblemTypeKeyNavigation).WithMany(p => p.CuttingDownAs)
                .HasForeignKey(d => d.ProblemTypeKey)
                .HasConstraintName("FK__Cutting_D__Probl__5DCAEF64");
        });

        modelBuilder.Entity<CuttingDownB>(entity =>
        {
            entity.HasKey(e => e.CuttingDownBIncidentId).HasName("PK__Cutting___C48B399F27D132BF");

            entity.ToTable("Cutting_Down_B", "STA");

            entity.Property(e => e.CuttingDownBIncidentId)
                .ValueGeneratedNever()
                .HasColumnName("Cutting_Down_B_Incident_ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser).HasMaxLength(100);
            entity.Property(e => e.CuttingDownCableName)
                .HasMaxLength(100)
                .HasColumnName("Cutting_Down_Cable_Name");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.PlannedEndDts)
                .HasColumnType("datetime")
                .HasColumnName("PlannedEndDTS");
            entity.Property(e => e.PlannedStartDts)
                .HasColumnType("datetime")
                .HasColumnName("PlannedStartDTS");
            entity.Property(e => e.ProblemTypeKey).HasColumnName("Problem_Type_Key");
            entity.Property(e => e.UpdatedUser).HasMaxLength(100);

            entity.HasOne(d => d.ProblemTypeKeyNavigation).WithMany(p => p.CuttingDownBs)
                .HasForeignKey(d => d.ProblemTypeKey)
                .HasConstraintName("FK_Cutting_Down_B_Problem_Type");
        });

        modelBuilder.Entity<CuttingDownDetail>(entity =>
        {
            entity.HasKey(e => e.CuttingDownDetailKey).HasName("PK__Cutting___DB17612B79AE6C68");

            entity.ToTable("Cutting_Down_Detail", "FTA");

            entity.HasIndex(e => new { e.CuttingDownKey, e.NetworkElementKey }, "IX_FTA_Cutting_Down_Detail_Header_Network");

            entity.Property(e => e.CuttingDownDetailKey)
                .ValueGeneratedNever()
                .HasColumnName("Cutting_Down_Detail_Key");
            entity.Property(e => e.ActualCreateDate).HasColumnType("datetime");
            entity.Property(e => e.ActualEndDate).HasColumnType("datetime");
            entity.Property(e => e.CuttingDownKey).HasColumnName("Cutting_Down_Key");
            entity.Property(e => e.NetworkElementKey).HasColumnName("Network_Element_Key");

            entity.HasOne(d => d.CuttingDownKeyNavigation).WithMany(p => p.CuttingDownDetails)
                .HasForeignKey(d => d.CuttingDownKey)
                .HasConstraintName("FK_Cutting_Down_Detail_Header");

            entity.HasOne(d => d.NetworkElementKeyNavigation).WithMany(p => p.CuttingDownDetails)
                .HasForeignKey(d => d.NetworkElementKey)
                .HasConstraintName("FK_Cutting_Down_Detail_Network_Element");
        });

        modelBuilder.Entity<CuttingDownHeader>(entity =>
        {
            entity.HasKey(e => e.CuttingDownKey).HasName("PK__Cutting___5629E0B007937EEF");

            entity.ToTable("Cutting_Down_Header", "FTA");

            entity.Property(e => e.CuttingDownKey)
                .ValueGeneratedNever()
                .HasColumnName("Cutting_Down_Key");
            entity.Property(e => e.ActualCreateDate).HasColumnType("datetime");
            entity.Property(e => e.ActualEndDate).HasColumnType("datetime");
            entity.Property(e => e.ChannelKey).HasColumnName("Channel_Key");
            entity.Property(e => e.CreateSystemUserId).HasColumnName("CreateSystemUserID");
            entity.Property(e => e.CuttingDownIncidentId).HasColumnName("Cutting_Down_Incident_ID");
            entity.Property(e => e.CuttingDownProblemTypeKey).HasColumnName("Cutting_Down_Problem_Type_Key");
            entity.Property(e => e.PlannedEndDts)
                .HasColumnType("datetime")
                .HasColumnName("PlannedEndDTS");
            entity.Property(e => e.PlannedStartDts)
                .HasColumnType("datetime")
                .HasColumnName("PlannedStartDTS");
            entity.Property(e => e.SynchCreateDate).HasColumnType("datetime");
            entity.Property(e => e.SynchUpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateSystemUserId).HasColumnName("UpdateSystemUserID");

            entity.HasOne(d => d.ChannelKeyNavigation).WithMany(p => p.CuttingDownHeaders)
                .HasForeignKey(d => d.ChannelKey)
                .HasConstraintName("FK_Cutting_Down_Header_Channel");

            entity.HasOne(d => d.CreateSystemUser).WithMany(p => p.CuttingDownHeaderCreateSystemUsers)
                .HasForeignKey(d => d.CreateSystemUserId)
                .HasConstraintName("FK_Cutting_Down_Header_CreateUser");

            entity.HasOne(d => d.CuttingDownProblemTypeKeyNavigation).WithMany(p => p.CuttingDownHeaders)
                .HasForeignKey(d => d.CuttingDownProblemTypeKey)
                .HasConstraintName("FK_Cutting_Down_Header_Problem_Type");

            entity.HasOne(d => d.CuttingDownProblemTypeKey1).WithMany(p => p.CuttingDownHeaders)
                .HasForeignKey(d => d.CuttingDownProblemTypeKey)
                .HasConstraintName("FK__Cutting_D__Cutti__71D1E811");

            entity.HasOne(d => d.UpdateSystemUser).WithMany(p => p.CuttingDownHeaderUpdateSystemUsers)
                .HasForeignKey(d => d.UpdateSystemUserId)
                .HasConstraintName("FK_Cutting_Down_Header_UpdateUser");
        });

        modelBuilder.Entity<CuttingDownIgnored>(entity =>
        {
            entity.HasKey(e => e.CuttingDownIncidentId).HasName("PK__Cutting___97DE53056EEBFF55");

            entity.ToTable("Cutting_Down_Ignored", "FTA");

            entity.Property(e => e.CuttingDownIncidentId)
                .ValueGeneratedNever()
                .HasColumnName("Cutting_Down_Incident_ID");
            entity.Property(e => e.ActualCreateDate).HasColumnType("datetime");
            entity.Property(e => e.CabelName)
                .HasMaxLength(100)
                .HasColumnName("Cabel_Name");
            entity.Property(e => e.CabinName)
                .HasMaxLength(100)
                .HasColumnName("Cabin_Name");
            entity.Property(e => e.CreatedUser).HasMaxLength(100);
            entity.Property(e => e.SynchCreateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Flat>(entity =>
        {
            entity.HasKey(e => e.FlatKey).HasName("PK__Flat__B7E8B7F7B85E05E8");

            entity.ToTable("Flat", "STA");

            entity.HasIndex(e => e.BuildingKey, "IX_STA_Flat_Building_Key");

            entity.Property(e => e.FlatKey)
                .ValueGeneratedNever()
                .HasColumnName("Flat_Key");
            entity.Property(e => e.BuildingKey).HasColumnName("Building_Key");

            entity.HasOne(d => d.BuildingKeyNavigation).WithMany(p => p.Flats)
                .HasForeignKey(d => d.BuildingKey)
                .HasConstraintName("FK_Flat_Building");
        });

        modelBuilder.Entity<Governrate>(entity =>
        {
            entity.HasKey(e => e.GovernrateKey).HasName("PK__Governra__7EE78ED556864895");

            entity.ToTable("Governrate", "STA");

            entity.Property(e => e.GovernrateKey)
                .ValueGeneratedNever()
                .HasColumnName("Governrate_Key");
            entity.Property(e => e.GovernrateName)
                .HasMaxLength(100)
                .HasColumnName("Governrate_Name");
        });

        modelBuilder.Entity<NetworkElement>(entity =>
        {
            entity.HasKey(e => e.NetworkElementKey).HasName("PK__Network___C7B9569607DA139E");

            entity.ToTable("Network_Element", "FTA");

            entity.HasIndex(e => e.ParentNetworkElementKey, "IX_FTA_Network_Element_Parent_Key");

            entity.HasIndex(e => e.NetworkElementTypeKey, "IX_FTA_Network_Element_Type_Key");

            entity.Property(e => e.NetworkElementKey)
                .ValueGeneratedNever()
                .HasColumnName("Network_Element_Key");
            entity.Property(e => e.NetworkElementName)
                .HasMaxLength(100)
                .HasColumnName("Network_Element_Name");
            entity.Property(e => e.NetworkElementTypeKey).HasColumnName("Network_Element_Type_Key");
            entity.Property(e => e.ParentNetworkElementKey).HasColumnName("Parent_Network_Element_Key");

            entity.HasOne(d => d.NetworkElementTypeKeyNavigation).WithMany(p => p.NetworkElements)
                .HasForeignKey(d => d.NetworkElementTypeKey)
                .HasConstraintName("FK_Network_Element_Type");

            entity.HasOne(d => d.ParentNetworkElementKeyNavigation).WithMany(p => p.InverseParentNetworkElementKeyNavigation)
                .HasForeignKey(d => d.ParentNetworkElementKey)
                .HasConstraintName("FK_Network_Element_Parent");
        });

        modelBuilder.Entity<NetworkElementHierarchyPath>(entity =>
        {
            entity.HasKey(e => e.NetworkElementHierarchyPathKey).HasName("PK__Network___EC59B2AB944C0674");

            entity.ToTable("Network_Element_Hierarchy_Path", "FTA");

            entity.Property(e => e.NetworkElementHierarchyPathKey)
                .ValueGeneratedNever()
                .HasColumnName("Network_Element_Hierarchy_Path_Key");
            entity.Property(e => e.Abbreviation).HasMaxLength(100);
            entity.Property(e => e.NetwrokElementHierarchyPathName)
                .HasMaxLength(500)
                .HasColumnName("Netwrok_Element_Hierarchy_Path_Name");
        });

        modelBuilder.Entity<NetworkElementType>(entity =>
        {
            entity.HasKey(e => e.NetworkElementTypeKey).HasName("PK__Network___9AE2A1112EE3B236");

            entity.ToTable("Network_Element_Type", "FTA");

            entity.HasIndex(e => e.NetworkElementHierarchyPathKey, "IX_FTA_Network_Element_Type_Hierarchy_Path");

            entity.HasIndex(e => e.ParentNetworkElementTypeKey, "IX_FTA_Network_Element_Type_Parent");

            entity.Property(e => e.NetworkElementTypeKey)
                .ValueGeneratedNever()
                .HasColumnName("Network_Element_Type_key");
            entity.Property(e => e.NetworkElementHierarchyPathKey).HasColumnName("Network_Element_Hierarchy_Path_Key");
            entity.Property(e => e.NetworkElementTypeName)
                .HasMaxLength(100)
                .HasColumnName("Network_Element_Type_Name");
            entity.Property(e => e.ParentNetworkElementTypeKey).HasColumnName("Parent_Network_Element_Type_key");

            entity.HasOne(d => d.NetworkElementHierarchyPathKeyNavigation).WithMany(p => p.NetworkElementTypes)
                .HasForeignKey(d => d.NetworkElementHierarchyPathKey)
                .HasConstraintName("FK_Network_Element_Type_Hierarchy_Path");

            entity.HasOne(d => d.ParentNetworkElementTypeKeyNavigation).WithMany(p => p.InverseParentNetworkElementTypeKeyNavigation)
                .HasForeignKey(d => d.ParentNetworkElementTypeKey)
                .HasConstraintName("FK_Network_Element_Type_Parent");
        });

        modelBuilder.Entity<ProblemType>(entity =>
        {
            entity.HasKey(e => e.ProblemTypeKey).HasName("PK__Problem___E6DB25E9B16F968E");

            entity.ToTable("Problem_Type", "FTA");

            entity.Property(e => e.ProblemTypeKey)
                .ValueGeneratedNever()
                .HasColumnName("Problem_Type_Key");
            entity.Property(e => e.ProblemTypeName)
                .HasMaxLength(100)
                .HasColumnName("problem_Type_Name");
        });

        modelBuilder.Entity<ProblemType1>(entity =>
        {
            entity.HasKey(e => e.ProblemTypeKey).HasName("PK__Problem___E6DB25E9F68CF2F8");

            entity.ToTable("Problem_Type", "STA");

            entity.Property(e => e.ProblemTypeKey)
                .ValueGeneratedNever()
                .HasColumnName("Problem_Type_Key");
            entity.Property(e => e.ProblemTypeName)
                .HasMaxLength(100)
                .HasColumnName("problem_Type_Name");
        });

        modelBuilder.Entity<Sector>(entity =>
        {
            entity.HasKey(e => e.SectorKey).HasName("PK__Sector__4D01A17ED2615083");

            entity.ToTable("Sector", "STA");

            entity.HasIndex(e => e.GovernrateKey, "IX_STA_Sector_Governrate_Key");

            entity.Property(e => e.SectorKey)
                .ValueGeneratedNever()
                .HasColumnName("Sector_Key");
            entity.Property(e => e.GovernrateKey).HasColumnName("Governrate_Key");
            entity.Property(e => e.SectorName)
                .HasMaxLength(100)
                .HasColumnName("Sector_Name");

            entity.HasOne(d => d.GovernrateKeyNavigation).WithMany(p => p.Sectors)
                .HasForeignKey(d => d.GovernrateKey)
                .HasConstraintName("FK_Sector_Governrate");
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.StationKey).HasName("PK__Station__32A17CF08917E342");

            entity.ToTable("Station", "STA");

            entity.HasIndex(e => e.CityKey, "IX_STA_Station_City_Key");

            entity.Property(e => e.StationKey)
                .ValueGeneratedNever()
                .HasColumnName("Station_Key");
            entity.Property(e => e.CityKey).HasColumnName("City_Key");
            entity.Property(e => e.StationName)
                .HasMaxLength(100)
                .HasColumnName("Station_Name");

            entity.HasOne(d => d.CityKeyNavigation).WithMany(p => p.Stations)
                .HasForeignKey(d => d.CityKey)
                .HasConstraintName("FK_Station_City");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionKey).HasName("PK__Subscrip__283EE706B0BAF1A9");

            entity.ToTable("Subscription", "STA");

            entity.HasIndex(e => e.FlatKey, "IX_STA_Subscription_Flat_Key");

            entity.Property(e => e.SubscriptionKey)
                .ValueGeneratedNever()
                .HasColumnName("Subscription_Key");
            entity.Property(e => e.BuildingKey).HasColumnName("Building_Key");
            entity.Property(e => e.FlatKey).HasColumnName("Flat_Key");
            entity.Property(e => e.MeterKey).HasColumnName("Meter_Key");
            entity.Property(e => e.PaletKey).HasColumnName("Palet_Key");

            entity.HasOne(d => d.BuildingKeyNavigation).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.BuildingKey)
                .HasConstraintName("FK_Subscription_Building");

            entity.HasOne(d => d.FlatKeyNavigation).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.FlatKey)
                .HasConstraintName("FK_Subscription_Flat");
        });

        modelBuilder.Entity<Tower>(entity =>
        {
            entity.HasKey(e => e.TowerKey).HasName("PK__Tower__6CF1B0149B2211B3");

            entity.ToTable("Tower", "STA");

            entity.HasIndex(e => e.StationKey, "IX_STA_Tower_Station_Key");

            entity.Property(e => e.TowerKey)
                .ValueGeneratedNever()
                .HasColumnName("Tower_Key");
            entity.Property(e => e.StationKey).HasColumnName("Station_Key");
            entity.Property(e => e.TowerName)
                .HasMaxLength(100)
                .HasColumnName("Tower_Name");

            entity.HasOne(d => d.StationKeyNavigation).WithMany(p => p.Towers)
                .HasForeignKey(d => d.StationKey)
                .HasConstraintName("FK_Tower_Station");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserKey).HasName("PK__Users__A2B887F2F23DF32A");

            entity.ToTable("Users", "FTA");

            entity.Property(e => e.UserKey)
                .ValueGeneratedNever()
                .HasColumnName("User_Key");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
        });

        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasKey(e => e.ZoneKey).HasName("PK__Zone__702254981679C735");

            entity.ToTable("Zone", "STA");

            entity.HasIndex(e => e.SectorKey, "IX_STA_Zone_Sector_Key");

            entity.Property(e => e.ZoneKey)
                .ValueGeneratedNever()
                .HasColumnName("Zone_Key");
            entity.Property(e => e.SectorKey).HasColumnName("Sector_Key");
            entity.Property(e => e.ZoneName)
                .HasMaxLength(100)
                .HasColumnName("Zone_Name");

            entity.HasOne(d => d.SectorKeyNavigation).WithMany(p => p.Zones)
                .HasForeignKey(d => d.SectorKey)
                .HasConstraintName("FK_Zone_Sector");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
