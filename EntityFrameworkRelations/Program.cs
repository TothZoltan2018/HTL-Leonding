using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

Console.WriteLine("Hello World");
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

// Table per Hierarchy. The base class and all the derived class will be put into a single DB table
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

    [Column(TypeName ="decimal(8,2)")]
    public decimal PriceEur { get; set; }
}