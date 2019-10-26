using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAnalyzer.Configuration
{
    public static class IocContainerConfiguration
    {
        public static void ConfigureService(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddTransient<IDataAnalyzerApp, DataAnalyzerApp>();

        }
    }
}
