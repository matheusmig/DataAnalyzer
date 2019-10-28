using DataAnalyzerModels;
using DataAnalyzerModels.Settings;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DataAnalyzerServices
{
    public class Job : IJob
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<FolderSettings> _folderSettings;
        private readonly IServiceProvider _serviceProvider;

        public Job(ILogger<IJob> logger, IOptionsMonitor<FolderSettings> folderSettings, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _folderSettings = folderSettings;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleNewFileAsync(string fileName, string inputFileFullPath)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var processor = services.GetRequiredService<IProcessor>();
            var cache = services.GetRequiredService<IDataWarehouse>();

            Report report = null;
            try
            {
                _logger.LogInformation($"{nameof(HandleNewFileAsync)} Start reading file : {fileName} : {DateTime.Now.TimeOfDay}");
                using var sr = File.OpenText(inputFileFullPath);
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
                _logger.LogError(ex, nameof(HandleNewFileAsync));
                return;
            }

            _logger.LogInformation($"{nameof(HandleNewFileAsync)} Start generating report : {fileName} : {DateTime.Now.TimeOfDay}");
            report = cache.GetReport();
            if (report == null)
            {
                _logger.LogWarning("Cannot generate a report");
                return;
            }

            try
            {
                _logger.LogInformation($"{nameof(HandleNewFileAsync)} Start writing report : {fileName} : {DateTime.Now.TimeOfDay}");

                var outputFilename = _folderSettings.CurrentValue.OutputPath + '\\' + fileName;

                using var writer = new StreamWriter(outputFilename, append: false);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(HandleNewFileAsync));
            }
        }
    }
}
