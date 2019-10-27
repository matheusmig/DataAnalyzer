using DataAnalyzerConstants;

namespace DataAnalyzerModels
{
    public class Client : IModel
    {
        public CodeIdentifier Code { get; set; }
        public string CNPJ { get; set; }
        public string Name { get; set; }
        public string BusinessArea { get; set; }
    }
}
