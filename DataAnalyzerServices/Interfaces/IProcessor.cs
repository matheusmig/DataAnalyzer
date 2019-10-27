using DataAnalyzerModels;
using System.IO;
using System.Threading.Tasks;

namespace DataAnalyzerServices.Interfaces
{
    public interface IProcessor
    {
        Task<IModel> ProcessLineAsync(string line);
    }
}
