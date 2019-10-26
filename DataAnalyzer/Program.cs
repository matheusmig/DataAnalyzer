using DataAnalyzer;
using DataAnalyzer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DesafioIlegra
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = BuildConfiguration();
            var services = new ServiceCollection();
            var serviceProvider = ConfigureServices(services, configuration);
            var app = serviceProvider.GetService<IDataAnalyzerApp>();
            Task.Run(() => app.Start()).Wait();
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            LoggingConfiguration.ConfigureService(services, configurationRoot);
            IocContainerConfiguration.ConfigureService(services, configurationRoot);

            return services.BuildServiceProvider();
        }
    }
}
