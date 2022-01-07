using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Console;

var factory = new CookbookContextFactory();
using var dbContext = factory.CreateDbContext();

var newDish = new Dish { Title = "Foo", Notes = "Bar"};
dbContext.Dishes.Add(newDish); // dbContext.Add(newDish); // seems ok, too
await dbContext.SaveChangesAsync();
newDish.Notes = "Baz";
await dbContext.SaveChangesAsync();

await EntityStates(factory);
await ChangeTracking(factory);
await AttachEntities(factory);
await NoTracking(factory);
await RawSql(factory);
await Transactions(factory);
await ExpressionTree(factory);

static async Task EntityStates(CookbookContextFactory factory)
{
    using var dbContext = factory.CreateDbContext();

    var newDish = new Dish { Title = "Foo", Notes = "Bar" };
    var state = dbContext.Entry(newDish).State; // << Detached

    dbContext.Dishes.Add(newDish); // dbContext.Add(newDish); // seems ok, too
    state = dbContext.Entry(newDish).State; // << Added

    await dbContext.SaveChangesAsync();
    state = dbContext.Entry(newDish).State; // << Unchanged

    newDish.Notes = "Baz";
    state = dbContext.Entry(newDish).State; // << Modified

    await dbContext.SaveChangesAsync();

    dbContext.Dishes.Remove(newDish);
    state = dbContext.Entry(newDish).State; // << Deleted

    await dbContext.SaveChangesAsync();
    state = dbContext.Entry(newDish).State; // << Detached

    ///////////////////////////// 21 : 52 //////////////////////////
}

static async Task ChangeTracking(CookbookContextFactory factory)
{
    using var dbContext = factory.CreateDbContext();

    var newDish = new Dish { Title = "Foo", Notes = "Bar"};
    dbContext.Dishes.Add(newDish);
    await dbContext.SaveChangesAsync();
    newDish.Notes = "Baz";

    var entry = dbContext.Entry(newDish);
    var originalValue = entry.OriginalValues[nameof(Dish.Notes)].ToString();
        var dishFromDatabase = await dbContext.Dishes.SingleAsync(d => d.Id == newDish.Id); // Reading a value Mem NOT from DB

    // --------------- a new dataContext -----------------
    using var dbContext2 = factory.CreateDbContext();    
    var dishFromDatabase2 = await dbContext2.Dishes.SingleAsync(d => d.Id == newDish.Id); // Reading a value from DB
}

static async Task AttachEntities(CookbookContextFactory factory)
{
    using var dbContext = factory.CreateDbContext();

    var newDish = new Dish { Title = "Foo", Notes = "Bar" };
    dbContext.Dishes.Add(newDish);
    await dbContext.SaveChangesAsync();

    // EF: Forget the "newDish" object
    dbContext.Entry(newDish).State = EntityState.Detached;
    var state = dbContext.Entry(newDish);

    dbContext.Dishes.Update(newDish); // Updates a previously unknown object if it was in the DB. 
    await dbContext.SaveChangesAsync();
}

static async Task NoTracking(CookbookContextFactory factory)
{
    using var dbContext = factory.CreateDbContext();

    // Select * from Dishes..
    // EF also stores all dishes in the original values to be able to track changes --> overhead
    // var dishes = await dbContext.Dishes.ToArrayAsync();
    // So, for Readonly scenarios to reduce overhead turn off tracking as below:
    var dishes = await dbContext.Dishes.AsNoTracking().ToArrayAsync();
    var state = dbContext.Entry(dishes[0]).State;
}

static async Task RawSql(CookbookContextFactory factory)
{
    using var dbContext = factory.CreateDbContext();

    var dishes = await dbContext.Dishes
        .FromSqlRaw("SELECT * FROM Dishes")
        .ToArrayAsync();

    var filter = "%z; DELETE FROM Dishes;";
    //var filter = "%z; DELETE FROM Dishes;"; // SQL attack!
    dishes = await dbContext.Dishes
        .FromSqlInterpolated($"SELECT * FROM Dishes WHERE Notes LIKE {filter}")
        .ToArrayAsync();
        
    // SQL Injection!!! --> SQL attack!
    dishes = await dbContext.Dishes
     .FromSqlRaw("SELECT * FROM Dishes WHERE Notes LIKE '" + filter + "'")
     .ToArrayAsync();
        
    // Writing the DB. This executed immediately on DB
    await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Dishes WHERE Id NOT IN (SELECT DishId FROM Ingredients)");
    
}

static async Task Transactions(CookbookContextFactory factory)
{
    using var dbContext = factory.CreateDbContext();

    using var transaction = await dbContext.Database.BeginTransactionAsync();
    try
    {
        dbContext.Dishes.Add(new Dish { Title = "TRFoo", Notes = "TRBar" });
        await dbContext.SaveChangesAsync();

        await dbContext.Database.ExecuteSqlRawAsync("SELECT 1/0 as Bad"); // This will throw an exception
        // If the code reaches here, writing to the DB will be executed
        await transaction.CommitAsync();
    }
    catch (SqlException ex)
    {
        Error.WriteLine($"Something bad happened: {ex.Message}");
    }
}

static async Task ExpressionTree(CookbookContextFactory factory)
{
    using var dbContext = factory.CreateDbContext();
    var newDish = new Dish { Title = "Foo", Notes = "Bar" };
    dbContext.Dishes.Add(newDish);
    await dbContext.SaveChangesAsync();

    var dishes = await dbContext.Dishes
        .Where(d => d.Title.StartsWith("F"))
        .ToArrayAsync();

    Func<Dish, bool> f = d => d.Title.StartsWith("F"); // Just a method
    // C# complirer is no longer generating machine language but object tree
    // Object tree can be inspected and translated to
    // a different language (eg. SQL) in runtime by tools such as EF.
    // The Where clause takes the Func into an Expression. (See the Where definition.)
    Expression<Func<Dish, bool>> ex = d => d.Title.StartsWith("F");

    var inMemoryDishes = new[]
    {
        new Dish { Title = "Foo", Notes = "Bar" },
        new Dish { Title = "Foo", Notes = "Bar" },
    };

    // Here the "normal" Where version will be called. (Without Expression) 
    dishes = inMemoryDishes
        .Where(d => d.Title.StartsWith("F"))
        .ToArray();
}


// Create model classes. They will become tables in the database.
// In more advanced scenarios (e.g. with inheritance, m:n relationships),
// the mapping becomes more complex.
// RECOMMENDATION: Use singlar for class names (NOT Dishes, BUT Dish)
class Dish
{
    // Note that by convention, a property named `Id` or `<type name>Id` will be 
    // configured as the primary key of an entity.
    // RECOMMENDATION: Add `Id` to each model class if you do not explicitly want 
    //   to have a different behavior.
    public int Id { get; set; }

    // Note that not-nullable reference type will result in a not-nullable column
    // in the database. 
    // RECOMMENDATION: Always turn Nullable Reference Types on in csproj and 
    //   control nullability via C# types.
    // RECOMMENDATION: Use Data Annotations (https://docs.microsoft.com/en-us/ef/core/modeling/entity-properties)
    //   to add additional metadata (e.g. maximum length, precision, etc.).
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    // This time we use a nullable type
    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? Stars { get; set; }

    // A dish consists of multiple ingredients. So, we use a `List` on this side
    // of the relation.
    // RECOMMENDATION: Always add a `List<T>` for the n-side of a relationship.
    public List<DishIngredient> Ingredients { get; set; } = new();
}

class DishIngredient
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Amount { get; set; }

    // Note that we reference the Dish that this ingredient relates to
    // by adding a property with the corresponding type.
    // RECOMMENDATION: Add such a property the 1-side of a relationship
    public Dish? Dish { get; set; }

    // Note the naming of the following property. It is <relation>Id.
    // Because of that, it will receive the foreign key value from the DB.
    // RECOMMENDATION: Add such a property the 1-side of a relationship
    public int? DishId { get; set; }
}

// In the DB context we add sets for each model class. We can skip
// entities which will not be used as the basis for queries (in our case
// 
class CookbookContext : DbContext
{
    // RECOMMENDATION: Use plural in the context (Dishes, NOT Dish)
    public DbSet<Dish> Dishes { get; set; }

    public DbSet<DishIngredient> Ingredients { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CookbookContext(DbContextOptions<CookbookContext> options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}

// This factory is responsible for creating our DB context. Note that
// this will NOT BE NECESSARY anymore once we move to ASP.NET.
class CookbookContextFactory : IDesignTimeDbContextFactory<CookbookContext>
{
    public CookbookContext CreateDbContext(string[]? args = null)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<CookbookContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new CookbookContext(optionsBuilder.Options);
    }
}
