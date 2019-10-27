using DataAnalyzerConstants;
using System.Collections.Generic;
using System.Linq;

namespace DataAnalyzerModels
{
    public class Sale : IModel
    {
        public CodeIdentifier Code { get; set; }
        public int SaleId { get; set; }
        public IEnumerable<Item> Items { get; set; }
        public string SalesmanName { get; set; }

        public decimal TotalPrice => Items.Sum(x => x.Price);
    }
}
