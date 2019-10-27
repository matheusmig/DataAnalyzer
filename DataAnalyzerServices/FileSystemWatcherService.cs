﻿using DataAnalyzerModels;
using DataAnalyzerModels.Settings;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IOptionsMonitor<FolderSettings> _folderSettings;

        private FileSystemWatcher _watcher;

        public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger, IServiceProvider serviceProvider,
            IOptionsMonitor<FolderSettings> folderSettings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _folderSettings = folderSettings;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing FileSystemWatcherService");

            _watcher = new FileSystemWatcher(_folderSettings.CurrentValue.InputPath);

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
            var services = scope.ServiceProvider;
            var processor = services.GetRequiredService<IProcessor>();
            var cache = services.GetRequiredService<IDataWarehouse>();
            
            Report report = null;
            string filename = null;
                       
            try
            {
                using var sr = File.OpenText(e.FullPath);
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(Created));
                return;
            }
           

            report = cache.GetReport();
            if (report == null)
            {
                _logger.LogWarning("Cannot generate a report");
                return;
            }
            
            try
            {
                var outputFilename = _folderSettings.CurrentValue.OutputPath + '\\' + e.Name;
                using var writer = new StreamWriter(outputFilename, append: false);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(Created));
            }        
        }
    }
}