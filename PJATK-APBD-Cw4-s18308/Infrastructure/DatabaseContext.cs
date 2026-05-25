using PJATK_APBD_Cw4_s18308.Entities;
using Microsoft.EntityFrameworkCore;

namespace PJATK_APBD_Cw4_s18308.Infrastructure;

public class DatabaseContext(DbContextOptions opt, IConfiguration configuration) : DbContext(opt)
{
    public virtual DbSet<PC> PCs { get; set; }
    public virtual DbSet<Component> Components { get; set; }
    public virtual DbSet<PCComponent> PCComponents { get; set; }
    public virtual DbSet<ComponentType> ComponentTypes { get; set; }
    public virtual DbSet<ComponentManufacturer> ComponentManufacturers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(configuration["DB:DefaultSchema"]);

        modelBuilder.Entity<PC>(opt =>
        {
            opt.HasKey(x => x.Id);

            opt.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            opt.Property(x => x.Weight)
                .HasColumnType("float(5)");

            opt.Property(x => x.CreatedAt)
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<ComponentManufacturer>(opt =>
        {
            opt.HasKey(x => x.Id);

            opt.Property(x => x.Abbreviation)
                .HasMaxLength(30)
                .IsRequired();

            opt.Property(x => x.FullName)
                .HasMaxLength(300)
                .IsRequired();
        });

        modelBuilder.Entity<ComponentType>(opt =>
        {
            opt.HasKey(x => x.Id);

            opt.Property(x => x.Abbreviation)
                .HasMaxLength(30)
                .IsRequired();

            opt.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();
        });

        modelBuilder.Entity<Component>(opt =>
        {
            opt.HasKey(x => x.Code);

            opt.Property(x => x.Code)
                .HasColumnType("char(10)");

            opt.Property(x => x.Name)
                .HasMaxLength(300)
                .IsRequired();

            opt.Property(x => x.Description)
                .HasColumnType("nvarchar(max)");

            opt.HasOne(x => x.ComponentManufacturer)
                .WithMany(x => x.Components)
                .HasForeignKey(x => x.ComponentManufacturersId);

            opt.HasOne(x => x.ComponentType)
                .WithMany(x => x.Components)
                .HasForeignKey(x => x.ComponentTypesId);
        });

        modelBuilder.Entity<PCComponent>(opt =>
        {
            opt.HasKey(x => new { x.PCId, x.ComponentCode });

            opt.Property(x => x.ComponentCode)
                .HasColumnType("char(10)");

            opt.HasOne(x => x.PC)
                .WithMany(x => x.PCComponents)
                .HasForeignKey(x => x.PCId);

            opt.HasOne(x => x.Component)
                .WithMany(x => x.PCComponents)
                .HasForeignKey(x => x.ComponentCode);
        });

        modelBuilder.Entity<ComponentManufacturer>().HasData(
            new ComponentManufacturer { Id = 1, Abbreviation = "AMD", FullName = "Advanced Micro Devices", FoundationDate = new DateOnly(1969, 5, 1) },
            new ComponentManufacturer { Id = 2, Abbreviation = "NV", FullName = "NVIDIA Corporation", FoundationDate = new DateOnly(1993, 4, 5) },
            new ComponentManufacturer { Id = 3, Abbreviation = "COR", FullName = "Corsair Gaming Inc.", FoundationDate = new DateOnly(1994, 1, 1) }
        );

        modelBuilder.Entity<ComponentType>().HasData(
            new ComponentType { Id = 1, Abbreviation = "CPU", Name = "Processor" },
            new ComponentType { Id = 2, Abbreviation = "GPU", Name = "Graphics Card" },
            new ComponentType { Id = 3, Abbreviation = "RAM", Name = "Memory" }
        );

        modelBuilder.Entity<Component>().HasData(
            new Component { Code = "CPU0000001", Name = "Ryzen 7 7800X3D", Description = "8-core gaming processor", ComponentManufacturersId = 1, ComponentTypesId = 1 },
            new Component { Code = "GPU0000001", Name = "RTX 4080 Super", Description = "High-end gaming graphics card", ComponentManufacturersId = 2, ComponentTypesId = 2 },
            new Component { Code = "RAM0000001", Name = "Corsair Vengeance DDR5 16GB", Description = "DDR5 RAM module 16GB", ComponentManufacturersId = 3, ComponentTypesId = 3 }
        );

        modelBuilder.Entity<PC>().HasData(
            new PC { Id = 1, Name = "Gaming Beast X", Weight = 12.5, Warranty = 36, CreatedAt = new DateTime(2026, 5, 8, 9, 0, 0), Stock = 5 },
            new PC { Id = 2, Name = "Office Mini Pro", Weight = 4.2, Warranty = 24, CreatedAt = new DateTime(2026, 4, 15, 13, 30, 0), Stock = 12 },
            new PC { Id = 3, Name = "Workstation Z", Weight = 18.0, Warranty = 48, CreatedAt = new DateTime(2026, 3, 1, 10, 0, 0), Stock = 3 }
        );

        modelBuilder.Entity<PCComponent>().HasData(
            new PCComponent { PCId = 1, ComponentCode = "CPU0000001", Amount = 1 },
            new PCComponent { PCId = 1, ComponentCode = "GPU0000001", Amount = 1 },
            new PCComponent { PCId = 1, ComponentCode = "RAM0000001", Amount = 2 },
            new PCComponent { PCId = 2, ComponentCode = "CPU0000001", Amount = 1 },
            new PCComponent { PCId = 2, ComponentCode = "RAM0000001", Amount = 1 },
            new PCComponent { PCId = 3, ComponentCode = "CPU0000001", Amount = 2 },
            new PCComponent { PCId = 3, ComponentCode = "GPU0000001", Amount = 2 }
        );
    }
}