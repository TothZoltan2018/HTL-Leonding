using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Creates a WebHost with default settings
// When an HHTP request comes in, it answers with "Hello World!"

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure(app =>
        {
            app.Run(async context =>
            {
                // Simulate BAD access of e.g. a DB.
                //Thread.Sleep(TimeSpan.FromSeconds(5));
                //Task.Delay(TimeSpan.FromSeconds(5)).Wait(); // As bad as the above line

                // Simulate GOOD access of e.g.a DB.
                await Task.Delay(TimeSpan.FromSeconds(10));

                await context.Response.WriteAsync("Hello World!");
            });
        });
    })
    .Build().Run();
