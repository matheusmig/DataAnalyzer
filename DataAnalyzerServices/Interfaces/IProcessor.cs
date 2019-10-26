using System.IO;
using System.Threading.Tasks;

namespace DataAnalyzerServices.Interfaces
{
    public interface IProcessor
    {
        Task ProcessLineAsync(string line);
    }
}
