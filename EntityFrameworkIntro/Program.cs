using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

var factory = new CookBookContextFactory();
using var context = factory.CreateDbContext(args);

Console.WriteLine("Add porridge for breakfast");
var porridge = new Dish { Title = "Breakfast Porridge", Notes = "This is so good", Stars = 4};
context.Dishes.Add(porridge); // context.Add(porridge); // OK ,too
await context.SaveChangesAsync();
Console.WriteLine($"Added porridge ({porridge.Id}) successfully");

Console.WriteLine("Checking stars for porridge");
var dishes = await context.Dishes
    .Where(d => d.Title.ToLower().Contains("porridge"))
    .ToListAsync(); //  Linq to SQL by EF. By ToListAsync() will the DB query be happening, not before.
if (dishes.Count != 1) Console.Error.WriteLine("Error: exactly 1 Porridge should be have been in the DB" );
Console.WriteLine($"Porridge has {dishes[0].Stars} stars");

Console.WriteLine("Update porridge stars to 5");
porridge.Stars = 5;
await context.SaveChangesAsync(); // by EF change tracker DB is updated
Console.WriteLine("Changed stars");

Console.WriteLine($"Removing porridge from DB");
context.Dishes.Remove(porridge); //context.Remove(porridge); // OK ,too
await context.SaveChangesAsync();
Console.WriteLine("Porridge removed");


// Model Classes
class Dish
{
    public int Id { get; set; } // Primary key (autoincremented) in DB

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; } // ? --> Nullable column in DB

    public int? Stars { get; set; } 

    public List<DishIngredient> Ingredients { get; set; } = new();
}

class DishIngredient
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5, 2)")] // Special to DB SQL Server
    public decimal Amount { get; set; }

    public Dish? Dish { get; set; } // Navigation property. 

    public int DishId { get; set; } // Foreign key value to the Dish. One of the primary keys in the Dish table    
}

// Databse Context. Entry point for working with the DB 
class CookbookContext : DbContext
{
    public DbSet<Dish> Dishes { get; set; } // DBSet represents a table in the DB
    public DbSet<DishIngredient> Ingredients { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CookbookContext(DbContextOptions<CookbookContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        : base(options)
    { }
}

// Creating a factory. Only needed for Command line apps
class CookBookContextFactory : IDesignTimeDbContextFactory<CookbookContext>
{
    public CookbookContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<CookbookContext>();
        optionsBuilder
             // Uncomment the following line
             // and install nuget package Microsoft.Extensions.Logging.Console
             // if you want to print generated SQL statements on the console.
             // (Not needed in ASP.NET)
             .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new CookbookContext(optionsBuilder.Options);
    }
}
