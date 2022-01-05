using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

Console.WriteLine("Haho");

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
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            // .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new CookbookContext(optionsBuilder.Options);
    }
}
