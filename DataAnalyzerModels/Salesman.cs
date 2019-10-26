namespace DataAnalyzerModels
{
    public class Salesman : IEntity
    {
        public int Id { get; set; }
        public string CPF { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
    }
}
