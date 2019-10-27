using DataAnalyzerModels;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyzerServices
{
    public class FileSystemWatcherService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        private FileSystemWatcher _watcher;

        public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing FileSystemWatcherService");

            var path = Directory.GetCurrentDirectory();
            _watcher = new FileSystemWatcher(path);

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            _watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Only watch text files.
            //_watcher.Filter = "*.txt";

            // Add event handlers.
            _watcher.Created += Created;
            _watcher.Changed += Changed;
            _watcher.Renamed += Rename;
            _watcher.Error += Error;

            _watcher.EnableRaisingEvents = true;

            return Task.CompletedTask;
        }

        private void Error(object sender, ErrorEventArgs e)
        {
            _logger.LogDebug("File error");
        }
        private void Rename(object sender, RenamedEventArgs e)
        { 
            _logger.LogDebug("File Renamed"); 
        }

        private void Changed(object sender, FileSystemEventArgs e) 
        { 
            _logger.LogDebug("File changed");           
        }

        private void Created(object sender, FileSystemEventArgs e) 
        {
            _logger.LogInformation("File created");

            using var scope = _serviceProvider.CreateScope();
            using var sr = File.OpenText(e.FullPath); 
            {
                try
                {
                    var services = scope.ServiceProvider;
                    var processor = services.GetRequiredService<IProcessor>();
                    var cache = services.GetRequiredService<IDataWarehouse>();
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();
                            var result = processor.ProcessLineAsync(line).Result;
                            if (result != null)
                            {
                                switch (result)
                                {
                                    case Salesman salesman:
                                        cache.Add(salesman);
                                        break;
                                    case Client client:
                                        cache.Add(client);
                                        break;
                                    case Sale sale:
                                        cache.Add(sale);
                                        break;
                                    default:
                                        _logger.LogError($"A instance of type {result.GetType().Name}");
                                        break;
                                }
                            }
                        }

                        var report = cache.GetReport();
                        _logger.LogInformation(JsonConvert.SerializeObject(report));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, nameof(Created));
                }

            }
        }
    }
}
