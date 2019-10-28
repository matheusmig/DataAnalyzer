using DataAnalyzerModels;

namespace DataAnalyzerServices.Interfaces
{
    public interface IProcessor
    {
        IModel ProcessLine(string line);
    }
}
