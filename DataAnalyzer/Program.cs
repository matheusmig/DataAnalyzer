﻿using DataAnalyzerModels.Settings;
using DataAnalyzerServices;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DataAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.AddTransient<IDataAnalyzerApp, DataAnalyzerApp>();
                services.AddTransient<IProcessor, Processor>();

                services.AddScoped<IDataWarehouse, DataWarehouse>();

                services.AddSingleton<IJobQueue, JobQueue>();

                services.AddHostedService<FileSystemWatcherService>();
                services.AddHostedService<QueuedHostedService>();

                services.ConfigureAll<FolderSettings>(settings =>
                {
                    settings.InputPath = System.Environment.GetEnvironmentVariable("HOMEPATH") + "\\data\\in";
                    settings.OutputPath = System.Environment.GetEnvironmentVariable("HOMEPATH") + "\\data\\out";
                });
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                configLogging.AddConsole();
            })
            .RunConsoleAsync();
        }
    }
}
