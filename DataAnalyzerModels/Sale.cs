using DataAnalyzerConstants;
using System.Collections.Generic;
using System.Linq;

namespace DataAnalyzerModels
{
    public class Sale : IModel
    {
        public CodeIdentifier Code => CodeIdentifier.Sale;
        public int SaleId { get; set; }
        public IEnumerable<Item> Items { get; set; }
        public string SalesmanName { get; set; }

        public decimal TotalPrice => Items != null ? Items.Sum(x => x.Price) : 0;
    }
}
