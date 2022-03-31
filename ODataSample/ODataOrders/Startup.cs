using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataOrders.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// C# ASP.NET 5 - Supercharging Our APIs With OData - Part 3 (Coding the OData Controller)
namespace ODataOrders
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<OdataOrdersContext>(options => options.UseSqlServer(
                Configuration["ConnectionStrings:DefaultConnection"]));

            // ### Config OData ###
            services.AddOData(opt => opt.Filter().Expand().Select().OrderBy()
                .AddModel("odata", GetEdmModel()));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // ### Config OData ### We have to tell oData, which tables are available over the internet
        private static IEdmModel GetEdmModel()
        {
            // Helper class with we can specify which tables we would like to publish.
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers"); // "Customers": this is the controlller's name
            return builder.GetEdmModel();
        }
    }
}
