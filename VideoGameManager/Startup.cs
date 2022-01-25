using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoGameManager.DataAccess;

namespace VideoGameManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration; // Access to appsettings.json
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // We configure here Dependency Injection. Service classes or services.
        public void ConfigureServices(IServiceCollection services)
        {
            // >>> Register Entity Framework so that It can be asked for from anywhere in the app (Dependency Injection)

            services.AddDbContext<VideoGameDataContext>(options => options.UseSqlServer(
                Configuration["ConnectionStrings:DefaultConnection"]));

            // Without this the frontend fails with CORS. Means that the UI and the API runs on different hosts.
            // This needs to be invoked in the Configure method.
            services.AddCors(options => options.AddDefaultPolicy(builder => 
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VideoGameManager", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // We add here middlewares
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VideoGameManager v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // If nobody feels responsibility for the http request then we got "not found" from the website.
        }
    }
}
