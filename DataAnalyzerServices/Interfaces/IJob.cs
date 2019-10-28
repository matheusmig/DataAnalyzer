using System.Threading.Tasks;

namespace DataAnalyzerServices.Interfaces
{
    public interface IJob
    {
        Task HandleNewFileAsync(string fileName, string inputFileFullPath);
    }
}
