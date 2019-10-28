using DataAnalyzerModels.Settings;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyzerServices
{
    public class FileSystemWatcherService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<FolderSettings> _folderSettings;
        private readonly IJobQueue _jobQueue;
        private readonly IJob _job;

        private FileSystemWatcher _watcher;

        public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger,
            IOptionsMonitor<FolderSettings> folderSettings, IJobQueue jobQueue, IJob job)
        {
            _logger = logger;
            _folderSettings = folderSettings;
            _jobQueue = jobQueue;
            _job = job;
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

            _jobQueue.QueueJob(async token => _job.HandleNewFileAsync(e.Name, e.FullPath));
        }

       
    }
}
