using System;
using System.Collections.Generic;
using System.Linq;

// C# 5 feature: namespace, main is not needed.
var result = GenerateNumbers(10)
    .Where(n =>
    {
        return n % 2 == 0;
    })
    .Select( n =>
    {
        return n * 3;
    });

foreach (var item in result)
{
    Console.WriteLine(item);
}

// This method is not called until the foreach above wants to get its result: "pull based approach"
// It generates a value and returns it immedately. Later it continues from here.
IEnumerable<int> GenerateNumbers(int maxValue)
{
    for (int i = 0; i <= maxValue; i++)
    {
        yield return i;
    }    
}
