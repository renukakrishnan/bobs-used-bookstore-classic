
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using System.Data.Entity;

    namespace Bookstore
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);
                
                // Configure additional configuration sources
                builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Configure database connection string
                var connectionString = builder.Configuration.GetConnectionString("BookstoreDatabaseConnection");

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews()
                    .AddViewOptions(options =>
                    {
                        options.HtmlHelperOptions.ClientValidationEnabled =
                            bool.Parse(builder.Configuration["ClientValidationEnabled"] ?? "true");
                    });

                // Register areas (formerly AreaRegistration.RegisterAllAreas())
                builder.Services.AddMvc()
                    .AddMvcOptions(options =>
                    {
                        // Add global filters that were previously registered in FilterConfig
                    });

// For bundles, you may want to consider using a bundling library like WebOptimizer
                // builder.Services.AddWebOptimizer(); // Uncomment if you add the WebOptimizer NuGet package

                // Configure logging (replacing NLog)
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.AddDebug();
                builder.Logging.AddEventSourceLogger();

                // Register Entity Framework (not upgraded to EFCore)
                // EntityFramework configuration is maintained but moved from web.config

                // Add other services from web.config
                builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

                // Add application settings
                builder.Services.Configure<AppSettings>(options =>
                {
                    options.Environment = builder.Configuration["Environment"] ?? "Development";
                    options.Services = new ServiceSettings
                    {
                        Authentication = builder.Configuration["Services/Authentication"] ?? "local",
                        Database = builder.Configuration["Services/Database"] ?? "local",
                        FileService = builder.Configuration["Services/FileService"] ?? "local",
                        ImageValidationService = builder.Configuration["Services/ImageValidationService"] ?? "local",
                        LoggingService = builder.Configuration["Services/LoggingService"] ?? "local"
                    };

                    // Authentication settings
                    options.Authentication = new AuthSettings
                    {
                        Cognito = new CognitoSettings
                        {
                            LocalClientId = builder.Configuration["Authentication/Cognito/LocalClientId"] ?? "",
                            AppRunnerClientId = builder.Configuration["Authentication/Cognito/AppRunnerClientId"] ?? "",
                            MetadataAddress = builder.Configuration["Authentication/Cognito/MetadataAddress"] ?? "",
                            CognitoDomain = builder.Configuration["Authentication/Cognito/CognitoDomain"] ?? ""
                        }
                    };

                    // File settings
                    options.Files = new FileSettings
                    {
                        BucketName = builder.Configuration["Files/BucketName"] ?? "",
                        CloudFrontDomain = builder.Configuration["Files/CloudFrontDomain"] ?? ""
                    };
                });

                //Added Services

                var app = builder.Build();
                
                // Configure the HTTP request pipeline (formerly Configure method)
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                
                //Added Middleware

                // Handle custom error logging (replacing Application_Error)
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        if (exception != null)
                        {
                            logger.LogError(exception, "Unhandled exception");
                        }

                        await Task.CompletedTask;
                    });
                });

                app.UseRouting();
                
                app.UseAuthorization();
                
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                app.Run();
            }
        }
        
        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }

        public class AppSettings
        {
            public string Environment { get; set; }
            public ServiceSettings Services { get; set; }
            public AuthSettings Authentication { get; set; }
            public FileSettings Files { get; set; }
        }

        public class ServiceSettings
        {
            public string Authentication { get; set; }
            public string Database { get; set; }
            public string FileService { get; set; }
            public string ImageValidationService { get; set; }
            public string LoggingService { get; set; }
        }

        public class AuthSettings
        {
            public CognitoSettings Cognito { get; set; }
        }

        public class CognitoSettings
        {
            public string LocalClientId { get; set; }
            public string AppRunnerClientId { get; set; }
            public string MetadataAddress { get; set; }
            public string CognitoDomain { get; set; }
        }

        public class FileSettings
        {
            public string BucketName { get; set; }
            public string CloudFrontDomain { get; set; }
        }
    }