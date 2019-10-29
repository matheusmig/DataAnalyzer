using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyzerServices.Interfaces
{
    public interface IJobBuilder
    {
        Task HandleNewFileAsync(string fileName, string inputFileFullPath, CancellationToken ct);
    }
}
