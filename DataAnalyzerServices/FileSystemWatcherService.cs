using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyzerServices
{
    public class FileSystemWatcherService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IProcessor _processor;

        private FileSystemWatcher _watcher;

        public FileSystemWatcherService(ILogger<FileSystemWatcherService> logger, IProcessor processor)
        {
            _logger = logger;
            _processor = processor;
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
            _watcher.Filter = "*.txt";

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
            _logger.LogInformation("File error");
        }
        private void Rename(object sender, RenamedEventArgs e)
        { 
            _logger.LogInformation("File Renamed"); 
        }

        private void Changed(object sender, FileSystemEventArgs e) 
        { 
            _logger.LogInformation("File changed");
            using (StreamReader sr = File.OpenText(e.FullPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    _processor.ProcessLineAsync(line);
                }
            }
        }

        private void Created(object sender, FileSystemEventArgs e) 
        {
            _logger.LogInformation("File created");
            using (StreamReader sr = File.OpenText(e.FullPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    _processor.ProcessLineAsync(line);
                }
            }
        }
    }
}
