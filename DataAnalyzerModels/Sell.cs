using System.Collections.Generic;

namespace DataAnalyzerModels
{
    public class Sell : IEntity
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public IEnumerable<Item> Items { get; set; }
        public Salesman Salesman { get; set; }
    }
}
