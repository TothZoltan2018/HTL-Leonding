using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

// C# 5 feature: namespace, main is not needed.
#region Part 1
var even = false;

var result = GenerateNumbers(10);

if (even == true)
{
    result = result.Where(n => n % 2 == 0); // deferred exexution.
}

result = result.OrderByDescending(n => n); // deferred exexution.

result = result.Select( n => n * 3); // deferred exexution.

// This calls method "GenerateNumbers".
//foreach (var item in result)
//{
//    Console.WriteLine(item);
//}

// This  calls method "GenerateNumbers" 'count' times.
Console.WriteLine(result.Count());

// This method is not called until the foreach above wants to get its result: "pull based approach"
// It generates a value and returns it immedately. Later it continues from here.
// This is because it returns an IEnumerable. It is called query composition. / Deferred execution.
IEnumerable<int> GenerateNumbers(int maxValue)
{
    for (int i = 0; i <= maxValue; i++)
    {
        yield return i;
    }    
}

int[] GetSquares(int exclusiveUpperLimit)
{
    //if (exclusiveUpperLimit > 46990) throw new OverflowException();

    return checked (GenerateNumbers(exclusiveUpperLimit)
        .Reverse()
        .Where(n => n % 7 == 0)
        .Select(x => x * x).ToArray());
}

var o = GetSquares(4700000);

Console.WriteLine();

#endregion

#region Part 2

var fileContent = await File.ReadAllTextAsync("MOCK_DATA.json");
var cars = JsonSerializer.Deserialize<CarData[]>(fileContent); // List or Array or IEnumerable

// Print all cars with at least 4 doors
var carsWithAtLeast4Doors = cars.Where(car => car.NumberOfDoors >= 4);
foreach (var car in carsWithAtLeast4Doors)
{
    Console.WriteLine($"The car {car.Model} has {car.NumberOfDoors} doors.");
}
Console.WriteLine();

// Print all Mazda cars with at least 4 doors
//var MazdaWithAtLeastFourDoors = cars.Where(car => car.NumberOfDoors >= 4 && car.Make == "Mazda"); //Slightly more performent in case on in memory objects
var MazdaWithAtLeastFourDoors = cars.Where(car => car.NumberOfDoors >= 4).Where(car => car.Make == "Mazda");
foreach (var car in MazdaWithAtLeastFourDoors)
{
    Console.WriteLine($"The Mazda car {car.Model} has {car.NumberOfDoors} doors.");
}
Console.WriteLine();

// Print a Make + Model for all Makes starts with "M"
cars.Where(car => car.Make.StartsWith('M'))
    .Select(car => $"{ car.Make} {car.Model}")
    .ToList()
    .ForEach(akarmi => Console.WriteLine(akarmi));
Console.WriteLine();

// Display the 10 most powerful cars (hp)
cars.OrderByDescending(car => car.HP)
    .Take(10)
    .Select(car => $"{ car.Make} {car.Model}")
    .ToList()
    .ForEach(akarmi => Console.WriteLine(akarmi));
Console.WriteLine();

#endregion

#region Part 3
// Display the number of models per make that apperared after 1995. My solution
cars.Where(car => car.Year >= 1995)
    .GroupBy(car => car.Make)
    .Select(g => $"{g.Key} has {g.Count()} Models after 1995")
    .ToList()
    .ForEach(x => Console.WriteLine(x));
Console.WriteLine();

cars.Where(car => car.Year >= 1995)
    .GroupBy(car => car.Make)
    .Select(c => new { c.Key, NumberOfModels = c.Count() })
    .ToList()
    .ForEach(item => Console.WriteLine($"{item.Key} has {item.NumberOfModels} Models after 1995"));
Console.WriteLine();

// Display the number of models per make that apperared after 2008.
// Makes should be displayed with "0" if there are no models after 2008.
cars.GroupBy(car => car.Make)
   .Select(c => new { c.Key, NumberOfModels = c.Count(c => c.Year > 2008) })
    .ToList()
    .ForEach(item => Console.WriteLine($"{item.Key} has {item.NumberOfModels} Models after 2008"));
Console.WriteLine("--------------------------------------------------------------------");

// Display a list of makes that have 2 models with HP 390..400 hp.
cars.Where(car => car.HP >= 390 && car.HP <= 400)
    .GroupBy(car => car.Make)
    .Select(car => new { car.Key, NumOfPowerFulModels = car.Count() })
    .Where(make => make.NumOfPowerFulModels >= 2)
    .ToList()
    .ForEach(make => Console.WriteLine(make.Key));
Console.WriteLine("--------------------------------------------------------------------");

// My version:
cars.Where(car => car.HP >= 390 && car.HP <= 400)
    .GroupBy(x => x.Make)
    .Where(x => x.Count() >= 2)
    .ToList()
    .ForEach(item => Console.WriteLine($"{item.Key} has at least 2 models with HP 390...400"));
Console.WriteLine("--------------------------------------------------------------------");

cars.Where(car => car.HP >= 390 && car.HP <= 400)
    .GroupBy(car => car.Make)
    .Where(x => x.Count() >= 2)
    .ToList()
    .ForEach(x =>
    {
        Console.WriteLine($"{x.Key} has at least 2 models with HP 390...400:");
        x.ToList().ForEach(x => Console.WriteLine($"\t{x.Make} {x.Model} {x.HP}"));
    });
Console.WriteLine("--------------------------------------------------------------------");

// Display the avarage HP per make
cars.GroupBy(car => car.Make)
    //.Select(x => new { Make = x.Key, HP = x.Select(x => x.HP).Average() })
    .Select(x => new { Make = x.Key, HP = x.Average(x => x.HP) })
    .ToList()
    .ForEach(x => Console.WriteLine($"{x.Make} made cars has an average of {x.HP} Hps"));
Console.WriteLine("--------------------------------------------------------------------");

// How many Makes built cars with HP between 0..100, 101.200, 201..300, 301..400, 401..500
// scitch is a C# 9 feaure
cars.GroupBy(car => car.HP switch
    {
    <= 100 => "0..100",
    <= 200 => "101..200",
    <= 300 => "201..300",
    <= 400 => "301..400",
       _ => "401..500"
    })
    .Select(car => new { HPCategory = car.Key, NumOfMake = car.Select(car => car.Make).Distinct().Count()})
    .ToList()
    .ForEach(item => Console.WriteLine($"{item.HPCategory}: {item.NumOfMake}"));    
Console.WriteLine("--------------------------------------------------------------------");


Console.WriteLine("--------------------------------------------------------------------");
Console.WriteLine("--------------------------------------------------------------------");
#endregion
class CarData
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("car_make")]
    public string Make { get; set; }
    [JsonPropertyName("car_model")]
    public string Model { get; set; }
    [JsonPropertyName("car_year")]
    public int Year { get; set; }
    [JsonPropertyName("number_of_doors")]
    public int NumberOfDoors { get; set; }
    [JsonPropertyName("hp")]
    public int HP { get; set; }
}


