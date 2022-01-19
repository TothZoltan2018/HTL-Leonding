using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

var factory = new BrickContextFactory();
using var context = factory.CreateDbContext();
using var context2 = factory.CreateDbContext();
//await AddData();
await QueryData();

async Task QueryData()
{
    // Query brickAvailability and the related Brick and Vendor data. (EF will SQL join the 3 tables)
    var brickAvailability = await context.BrickAvailabilities
        .Include(ba => ba.Brick)
        .Include(ba => ba.Vendor)
        .ToArrayAsync();

    foreach (var item in brickAvailability)
    {
        Console.WriteLine($"Brick { item.Brick.Title} available at {item.Vendor.VendorName} for {item.PriceEur}");
    }

    Console.WriteLine("-----------------------------------------------------------------------------");

    var bricksWithVendorsAndTags = await context.Bricks
        //.Where(b => b.Tags.Any(t => t.Title == "Minecraft")) // It implicitely uses Include        
        .Include(nameof(Brick.Availability) + "." + nameof(BrickAvailability.Vendor))
        .Include(b => b.Tags)
        .ToArrayAsync();

    foreach (var item in bricksWithVendorsAndTags)
    {
        Console.Write($"Brick {item.Title} ");
        if (item.Tags.Any()) Console.Write($"({ string.Join(", ", item.Tags.Select(t => t.Title)) })");
        if (item.Availability.Any()) Console.WriteLine($" is available at { string.Join(", ", item.Availability.Select(a => a.Vendor.VendorName))}! ");
    }

    Console.WriteLine("-------------------------------- Explicitly loading AFTER the linq query -------------------------------------");

    var simpleBricks = await context2.Bricks.ToArrayAsync();
    foreach (var item in simpleBricks)
    {
        // like var simpleBricks = await context2.Bricks..Include(b => b.Tags).ToArrayAsync();
        await context2.Entry(item).Collection(i => i.Tags).LoadAsync();
        Console.Write($"Brick {item.Title} ");
        if (item.Tags.Any()) Console.Write($"({ string.Join(", ", item.Tags.Select(t => t.Title)) })");
    }
}

async Task AddData()
{
    Vendor brickKing, heldDerSteine;
    await context.AddRangeAsync(new[]
        {
            brickKing =  new Vendor() { VendorName = "Brick King" },
            heldDerSteine = new Vendor() { VendorName = "held Der Steine" }
        });
    //await context.SaveChangesAsync();

    Tag rare, ninjago, minecraft;
    await context.AddRangeAsync(new[]
    {
        rare = new Tag() { Title = "Rare" },
        ninjago = new Tag() { Title = "Ninjago" },
        minecraft = new Tag() { Title = "Minecraft" },
    });
    //await context.SaveChangesAsync();

    await context.AddAsync(new BasePlate
    {
        Title = "BasePlate 16 x 16 with blue water pattern",
        Color = Color.Green,
        Tags = new() { rare, minecraft },
        Length = 16,
        Width = 16,
        Availability = new()
        {
            new() { Vendor = brickKing, AvailableAmount = 5, PriceEur = 6.5m },
            new() { Vendor = heldDerSteine, AvailableAmount = 10, PriceEur = 5.9m },
        }
    });
    await context.SaveChangesAsync();
}



#region Model
// 1. Add Nuget packages:
//  Microsoft.EntityFrameworkCore.Design
//  Microsoft.EntityFrameworkCore.SqlServer
//  Because this is not an ASP.NET app we need these 2, too:
//      Microsoft.Extensions.Configuration.Json (to read the appsettings file for connectionstring)
//      Microsoft.Extensions.Logging.Console (to look the generated SQL statements in the console)

enum Color
{
    Black,
    White,
    Red,
    Yellow,
    Orange,
    Green
}

class Brick
{
    public int Id { get; set; }

    [MaxLength(250)]
    public string Title { get; set; } = String.Empty;

    public Color? Color { get; set; }

    public List<Tag> Tags { get; set; } = new(); // N part of relationship to table "Tag"
    public List<BrickAvailability> Availability { get; set; } = new(); // N part of the relation
}

// For adding a many to many relationship before EF 5.0 having a separate table (class) was needed.
// From EF 5.0 it is not needed. Now, between the 2 classes the relationship is done by adding
// cross referencing properties with type List<the other class>.

// Table per Hierarchy. The base class and all the derived classes will be put into a single DB table
// The "Discriminator" column will contain the actual class name, such as "BasePlate" or "MinifigHead"

// N to N relationships are done by a DB table "Brick.Tag" connecting the two tables. Two columns:
//      Foreign keys: BricksId, TagsId
class BasePlate : Brick
{
    public int Length { get; set; }
    public int Width { get; set; }
}

class MinifigHead : Brick
{
    public bool IsDualSided { get; set; }
}
class Tag
{
    public int Id { get; set; }

    [MaxLength(250)]
    public string Title { get; set; } = String.Empty;

    public List<Brick> Bricks { get; set; } = new(); // N part of relationship to table "Brick"
}

class Vendor
{
    public int Id { get; set; }

    [MaxLength(250)]
    public string VendorName { get; set; }
    public List<BrickAvailability> Availability { get; set; } = new();// N part of the relation
}

class BrickAvailability
{
    public int Id { get; set; }

    public Vendor Vendor { get; set; } // One part of the relation

    public int VendorId { get; set; } // FK to Vendor table

    public Brick Brick { get; set; } // One part of the relation

    public int BrickId { get; set; } // FK to Brick table

    public int AvailableAmount { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal PriceEur { get; set; }
}


#endregion

#region Data Context
class BrickContext : DbContext
{
    public BrickContext(DbContextOptions<BrickContext> options)
        : base(options)
    { }

    public DbSet<Brick> Bricks { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<BrickAvailability> BrickAvailabilities { get; set; }
    public DbSet<Tag> Tags { get; set; }

    // EF need to store the two derived tables, too
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BasePlate>().HasBaseType<Brick>();
        modelBuilder.Entity<MinifigHead>().HasBaseType<Brick>();
    }
}

class BrickContextFactory : IDesignTimeDbContextFactory<BrickContext>
{
    public BrickContext CreateDbContext(string[] args = null)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<BrickContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new BrickContext(optionsBuilder.Options);
    }
}
#endregion
