using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DataAnalyzer
{
    public class DataAnalyzerApp : IDataAnalyzerApp
    {
        private readonly ILogger _logger;

        public DataAnalyzerApp(ILogger<IDataAnalyzerApp> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Starting DataAnalyzerApp");

            // throw new NotImplementedException();

            await Task.Delay(TimeSpan.FromSeconds(10));
            return;
        }

    }
}
