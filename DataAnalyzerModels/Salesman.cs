using DataAnalyzerConstants;

namespace DataAnalyzerModels
{
    public class Salesman : IModel
    {
        public CodeIdentifier Code => CodeIdentifier.Salesman;
        public string CPF { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
    }
}
