using DataAnalyzerModels;

namespace DataAnalyzerServices.Interfaces
{
    public interface IDataWarehouse
    {
        void Add<T>(T model) where T : IModel;

        public Report GetReport();
    }
}
