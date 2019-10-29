using DataAnalyzerModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DataAnalyzerServices.Interfaces
{
    public class DataWarehouse : IDataWarehouse
    {
        private readonly ILogger _logger;

        private int _clientCount;
        private int _salesmanCount;

        private readonly object _mostExpensiveSalePriceLock;
        private decimal _mostExpensiveSalePrice;
        private Sale _mostExpensiveSale;

        private readonly object _salesmansLock;
        private IList<Salesman> _salesmans;
        private ConcurrentDictionary<Salesman, decimal> _pricesBySalesman;

        public DataWarehouse(ILogger<IDataWarehouse> logger)
        {
            _logger = logger;

            _clientCount = 0;
            _salesmanCount = 0;
            _mostExpensiveSalePriceLock = new object();
            _mostExpensiveSalePrice = decimal.MinValue;
            _mostExpensiveSale = null;

            _salesmansLock = new object();
            _salesmans = new List<Salesman>();
            _pricesBySalesman = new ConcurrentDictionary<Salesman, decimal>();
        }

        public bool TryAdd<T>(T model) where T : IModel
        {
            switch (model)
            {
                case Client client:
                    {
                        Interlocked.Increment(ref _clientCount);
                        return true;
                    }                    
                case Salesman salesman:
                    {
                        Interlocked.Increment(ref _salesmanCount);

                        lock (_salesmansLock)
                            _salesmans.Add(salesman);
                        
                        return true;
                    }
                case Sale sale:
                    {
                        var totalPrice = sale.TotalPrice;
                        lock (_mostExpensiveSalePriceLock)
                        {
                            if (totalPrice > _mostExpensiveSalePrice)
                            {
                                _mostExpensiveSalePrice = totalPrice;
                                _mostExpensiveSale = sale;
                            }
                        }

                        lock (_salesmansLock)
                        {
                            var salesman = _salesmans.Where(x => x.Name == sale.SalesmanName).FirstOrDefault();
                            if (salesman != null)
                            _pricesBySalesman.AddOrUpdate(salesman, totalPrice, 
                                (key, existingValue) =>  existingValue += totalPrice);
                        }

                        return true;
                    }
                default:
                    {
                        _logger.LogWarning($"{nameof(TryAdd)} Unknown model");
                        return false;
                    }
            }
        }

        public Report GetReport()
        {
            return new Report()
            {
                ClientCount = _clientCount,
                SalesmanCount = _salesmanCount,
                MostExpensiveSaleId = _mostExpensiveSale?.SaleId ?? 0,
                WorstSalesman = _pricesBySalesman
                .OrderByDescending(o => o.Key.Salary > 0 ? (o.Value / o.Key.Salary) : decimal.MinValue)
                .Select(x => x.Key)
                .FirstOrDefault()?.Name
            };
        }
    }
}
