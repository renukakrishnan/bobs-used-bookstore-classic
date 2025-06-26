
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bookstore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add connection string from Web.config
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string> {
                { "ConnectionStrings:BookstoreDatabaseConnection", "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=BookStoreClassic;MultipleActiveResultSets=true;Integrated Security=SSPI;" }
            });

            // Add appSettings from Web.config
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string> {
                { "Environment", "Development" },
                { "Services:Authentication", "local" },
                { "Services:Database", "local" },
                { "Services:FileService", "local" },
                { "Services:ImageValidationService", "local" },
                { "Services:LoggingService", "local" },
                { "Authentication:Cognito:LocalClientId", "[Retrieved from AWS Systems Manager Parameter Store when Services/Authentication == 'aws']" },
                { "Authentication:Cognito:AppRunnerClientId", "[Retrieved from AWS Systems Manager Parameter Store when Services/Authentication == 'aws']" },
                { "Authentication:Cognito:MetadataAddress", "[Retrieved from AWS Systems Manager Parameter Store when Services/Authentication == 'aws']" },
                { "Authentication:Cognito:CognitoDomain", "[Retrieved from AWS Systems Manager Parameter Store when Services/Authentication == 'aws']" },
                { "Files:BucketName", "[Retrieved from AWS Systems Manager Parameter Store when Services/FileService == 'aws']" },
                { "Files:CloudFrontDomain", "[Retrieved from AWS Systems Manager Parameter Store when Services/FileService == 'aws']" }
            });

            // Store configuration in static ConfigurationManager
            ConfigurationManager.Configuration = builder.Configuration;

            // Add services to the container (formerly ConfigureServices)
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Register filters
            builder.Services.AddMvc(options =>
            {
                // Add global filters here (equivalent to FilterConfig.RegisterGlobalFilters)
            });

                // Add Bundling and Minification (equivalent to BundleConfig.RegisterBundles)
                // .NET Core uses built-in bundling and minification via WebOptimizer or other similar libraries
                // You would need to add WebOptimizer package if needed

                // Configure Entity Framework (from Web.config)
                // Note: This registration assumes that EntityFramework is referenced properly
                // and the necessary services are included in your project

                // Add support for client validation
                builder.Services.AddMvc().AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = true;
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
                
                app.UseRouting();
                
                app.UseAuthorization();
                
                // Configure client validation
                builder.Services.Configure<MvcOptions>(options =>
                {
                    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
                        (x) => $"The value '{x}' is invalid.");
                });

                // Register areas (equivalent to AreaRegistration.RegisterAllAreas())
                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                // Register routes (equivalent to RouteConfig.RegisterRoutes)
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.MapRazorPages();

                // Configure global error handling (equivalent to Application_Error)
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        if (exception != null)
                        {
                            logger.LogError(exception, "An unhandled exception occurred.");
                        }

                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsync("An unexpected error occurred.");
                    });
                });

                app.Run();
            }
        }
        
        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }
    }