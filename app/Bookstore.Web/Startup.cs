using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Bookstore.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationSetup.ConfigureConfiguration(_configuration);

            DependencyInjectionSetup.ConfigureDependencyInjection(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            AuthenticationConfig.ConfigureAuthentication(app);
        }
    }

    public static class ConfigurationSetup
    {
        public static void ConfigureConfiguration(IConfiguration configuration)
        {
            // Configuration setup logic here
        }
    }

    public static class DependencyInjectionSetup
    {
        public static void ConfigureDependencyInjection(IServiceCollection services)
        {
            // Dependency injection setup logic here
        }
    }

    public static class AuthenticationConfig
    {
        public static void ConfigureAuthentication(IApplicationBuilder app)
        {
            // Authentication configuration logic here
        }
    }
}
