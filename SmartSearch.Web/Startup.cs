using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartSearch.Core;
using SmartSearch.Core.Interfaces;
using SmartSearch.Core.Services;
using System;

namespace SmartSearch.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", configuration["AWS:AccessKey"]);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", configuration["AWS:SecretKey"]);
            Environment.SetEnvironmentVariable("AWS_REGION", configuration["AWS:Region"]);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<ISearchClient, ElasticSearchClient>();
            services.AddTransient<IDataService, DataService>();
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
    }
}
