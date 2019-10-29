using DataAnalyzerModels;

namespace DataAnalyzerServices.Interfaces
{
    public interface IDataWarehouse
    {
        bool TryAdd<T>(T model) where T : IModel;

        public Report GetReport();
    }
}
