using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#region Sync
//var lines = File.ReadAllLines("TextFile1.txt");

//foreach (var line in lines)
//{
//    Console.WriteLine(line);
//}
#endregion

#region Async but blocking way (wrong)
//var fileReadTask = File.ReadAllLinesAsync("TextFile1.txt");

//fileReadTask.Wait(); // Blocks the thread. It makes synchronous
//var lines = fileReadTask.Result; // Blocks the thread. It makes synchronous
#endregion

#region Async old way
//File.ReadAllLinesAsync("TextFile1.txt")
//    .ContinueWith(t =>
//    {
//        if (t.IsFaulted)
//        {
//            Console.Error.WriteLine(t.Exception);            
//        }
//        // It runs when the Task is completed!
//        foreach (var line in t.Result)
//        {
//            Console.WriteLine(line);
//        }
//    });

// Console.ReadKey();
#endregion

#region Async new way
//async Task ReadFile()
//{
//    var lines = await File.ReadAllLinesAsync("TextFile1.txt");

//    foreach (var line in lines)
//    {
//        Console.WriteLine(line);
//    }
//}

//await ReadFile();

async Task<int> GetDataFromNetworkAsync()
{
    // Simulate network call
    await Task.Delay(150);
    var result = 42;
    return result;
}

var networkResult = await GetDataFromNetworkAsync();

Func<Task<int>> GetDataFromNetworkViaLambda = async () =>
{
    // Simulate network call
    await Task.Delay(150);
    var result = 42;
    return result;
};


#endregion