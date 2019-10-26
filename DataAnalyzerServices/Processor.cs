using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace DataAnalyzerServices
{
    public class Processor : IProcessor
    {
        private readonly ILogger _logger;
        public Processor(ILogger<IProcessor> logger)
        {
            _logger = logger;
        }

        public async Task ProcessLineAsync(string line)
        {
            _logger.LogInformation($"Received Line: {line}");
        }
    }
}
