using System;
using System.Net.Http;
using Landmark.FloodData.Gateway;
using Landmark.FloodData.Processor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Landmark.FloodData
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
            services.AddControllers()
                .AddNewtonsoftJson();

            services.AddHttpClient("EnvironmentAgency", client =>
            {
                client.BaseAddress = new Uri(Configuration["EnvironmentAgencyBaseUrl"]);
            });

            services.AddScoped<HttpMessageHandler, HttpClientHandler>();
            services.AddScoped<IEnvironmentAgencyGateway, EnvironmentAgencyGateway>();
            services.AddScoped<FloodDataProcessor, FloodDataProcessor>();
            services.AddScoped<IFloodActionStrategy, HardCodedFloodActionStrategy>();
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
