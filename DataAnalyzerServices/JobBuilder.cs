using DataAnalyzerModels;
using DataAnalyzerModels.Settings;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyzerServices
{
    public class JobBuilder : IJobBuilder
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<FolderSettings> _folderSettings;
        private readonly IServiceProvider _serviceProvider;

        public JobBuilder(ILogger<IJobBuilder> logger, IOptionsMonitor<FolderSettings> folderSettings, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _folderSettings = folderSettings;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleNewFileAsync(string fileName, string inputFileFullPath, CancellationToken ct)
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
                        ct.ThrowIfCancellationRequested();

                        var line = await sr.ReadLineAsync();
                        var result = processor.ProcessLine(line);
                        if (result != null)
                        {
                            switch (result)
                            {
                                case Salesman salesman:
                                    cache.TryAdd(salesman);
                                    break;
                                case Client client:
                                    cache.TryAdd(client);
                                    break;
                                case Sale sale:
                                    cache.TryAdd(sale);
                                    break;
                                default:
                                    _logger.LogError($"A instance of type {result.GetType().Name}");
                                    break;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException opCancEx)
            {
                _logger.LogWarning(opCancEx, "Operation canceled");
                return;
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
