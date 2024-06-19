using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using WebApi.Middlewares;
using WebApi.Services;
using WebApi.Services.AlphaVantageService;
using WebApi.Services.HttpClientWrapper;

namespace WebApi
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
            #region Swagger
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(string.Format(@"{0}\TestProject.xml", System.AppDomain.CurrentDomain.BaseDirectory));
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "TestProject",
                });
            });

            
            services.AddSwaggerGenNewtonsoftSupport();
            #endregion
            #region Api Versioning
            // Add API Versioning to the Project
            services.AddApiVersioning(config =>
            {
                // Specify the default API Version as 1.0
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
            #endregion

            // Registering HttpClientWrapper as a singleton so it can be injected wherever IHttpClientWrapper is needed
            // This is for testing purposes
            services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
            services.AddSingleton<IAlphaVantageService, AlphaVantageService>();
            // Registering PriceUpdateService as a singleton for IPriceUpdateService to allow it to be injected
            // This is for testing purposes
            services.AddSingleton<IPriceUpdateService, PriceUpdateService>();
            // Registering PriceUpdateService as a hosted service so it runs in the background
            services.AddHostedService<PriceUpdateService>();
            // Register HttpClient to be used by HttpClientWrapper
            services.AddHttpClient<HttpClientWrapper>();
            
            services.AddControllers();
            services.AddHttpClient();
            services.AddLogging(configure => configure.AddConsole())
                    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);

        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseWebSockets();
            app.UseMiddleware<WebSocketMiddleware>();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            #region Swagger
            app.UseSwagger();            
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestProject");
            });
            #endregion
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
