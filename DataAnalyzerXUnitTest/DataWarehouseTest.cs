using DataAnalyzerModels;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DataAnalyzerXUnitTest
{
    public class DataWarehouseTest
    {
        private Mock<ILogger<IDataWarehouse>> logger;

        private void DefaultArrange()
        {
            logger = new Mock<ILogger<IDataWarehouse>>(MockBehavior.Loose);
        }

        public static IEnumerable<object[]> GetValidMockData()
        {
            return new List<object[]>
            {
                new object[] { new Salesman()},
                new object[] { new Sale()},
                new object[] { new Client()}
            };
        }

        public static IEnumerable<object[]> GetInvalidMockData()
        {
            return new List<object[]>
            {
                new object[] { new Item()},
                new object[] { new Report()},
            };
        }

        public static IEnumerable<object[]> GetValidReportdMockData()
        {
            return new List<object[]>
            {
                new object[] { 1, 0, 0, null, new object[] { new Client() { Name = "Alfredo", BusinessArea = "Rural", CNPJ = "5521589856325" } } },
                new object[] { 0, 1, 0, null, new object[] { new Salesman() { CPF = "111111111", Name = "Pedro", Salary = 500.10m } } },
                new object[] { 0, 0, 1, null, new object[] {
                    new Sale() {SaleId = 1, SalesmanName = "Alo" , Items = new List<Item>(){
                        new Item() { ItemId = 1, Price = 50.5m, Quantity = 80},
                        new Item() { ItemId = 2, Price = 300.5m, Quantity = 90},
                }}}},

                new object[] { 5, 0, 0, null,new object[] {
                    new Client() { Name = "Alfredo", BusinessArea = "Rural", CNPJ = "5521589856325" },
                    new Client() { Name = "Fausto", BusinessArea = "Urbana", CNPJ = "8789842014444" },
                    new Client() { Name = "Argel", BusinessArea = "Rural", CNPJ = "7441410001111" },
                    new Client() { Name = "Chaves", BusinessArea = "Urbana", CNPJ = "1939203940191" },
                    new Client() { Name = "Fausto", BusinessArea = "Urbana", CNPJ = "4920349490201" },
                }},
                new object[] { 0, 5, 0, null, new object[] {
                    new Salesman() { CPF = "111111111", Name = "Pedro", Salary = 600.10m },
                    new Salesman() { CPF = "222222222", Name = "Afonso", Salary = 700.10m },
                    new Salesman() { CPF = "333333333", Name = "Ingram", Salary = 900.10m },
                    new Salesman() { CPF = "444444444", Name = "Kazuma", Salary = 400.10m },
                    new Salesman() { CPF = "998988888", Name = "Lucio", Salary = 800.10m }} },

                new object[] { 5, 5, 5, "Ingram", new object[] {
                    new Client() { Name = "Alfredo", BusinessArea = "Rural", CNPJ = "5521589856325" },
                    new Client() { Name = "Fausto", BusinessArea = "Urbana", CNPJ = "8789842014444" },
                    new Client() { Name = "Argel", BusinessArea = "Rural", CNPJ = "7441410001111" },
                    new Client() { Name = "Chaves", BusinessArea = "Urbana", CNPJ = "1939203940191" },
                    new Client() { Name = "Fausto", BusinessArea = "Urbana", CNPJ = "4920349490201" },
                    new Salesman() { CPF = "111111111", Name = "Pedro", Salary = 600.10m },
                    new Salesman() { CPF = "222222222", Name = "Afonso", Salary = 700.10m },
                    new Salesman() { CPF = "333333333", Name = "Ingram", Salary = 5.33m },
                    new Salesman() { CPF = "444444444", Name = "Kazuma", Salary = 400.10m },
                    new Salesman() { CPF = "998988888", Name = "Lucio", Salary = 800.10m },
                    new Sale() {SaleId = 1, SalesmanName = "Pedro" , Items = new List<Item>(){
                        new Item() { ItemId = 9, Price = 50.5m, Quantity = 80},
                        new Item() { ItemId = 8, Price = 300.5m, Quantity = 90},
                    }},
                    new Sale() {SaleId = 2, SalesmanName = "Afonso" , Items = new List<Item>(){
                        new Item() { ItemId = 9, Price = 50.5m, Quantity = 80},
                        new Item() { ItemId = 8, Price = 300.5m, Quantity = 90},
                    }},
                    new Sale() {SaleId = 3, SalesmanName = "Ingram" , Items = new List<Item>(){
                        new Item() { ItemId = 9, Price = 400.5m, Quantity = 80},
                        new Item() { ItemId = 8, Price = 300.5m, Quantity = 90},
                    }},
                    new Sale() {SaleId = 4, SalesmanName = "Kazuma" , Items = new List<Item>(){
                        new Item() { ItemId = 9, Price = 50.5m, Quantity = 80},
                        new Item() { ItemId = 8, Price = 300.5m, Quantity = 90},
                    }},
                    new Sale() {SaleId = 5, SalesmanName = "Lucio" , Items = new List<Item>(){
                        new Item() { ItemId = 1, Price = 111.5m, Quantity = 350},
                        new Item() { ItemId = 2, Price = 111, Quantity = 444},
                    }}}
                }                 
            };
        }

        [Theory(DisplayName = "Add Model")]
        [MemberData(nameof(GetValidMockData))]
        public void TestAddValidModel(object model)
        {
            // Arrange
            DefaultArrange();
            var service = new DataWarehouse(logger.Object);

            // Act
            var addResult = service.TryAdd(model as IModel);
            
            // Assert
            Assert.True(addResult);
        }

        [Theory(DisplayName = "Add invalid Model")]
        [MemberData(nameof(GetInvalidMockData))]
        public void TestAddInvalidModel(object model)
        {
            // Arrange
            DefaultArrange();
            var service = new DataWarehouse(logger.Object);

            // Act
            var addResult = service.TryAdd(model as IModel);

            // Assert
            Assert.False(addResult);
        }

        [Theory(DisplayName = "Get valid Report")]
        [MemberData(nameof(GetValidReportdMockData))]
        public void TestGetReportValid(int clientCount, int salesmanCount, int mostExpensiveSaleId, string worstSalesman,
            IEnumerable<object> models)
        {
            // Arrange
            DefaultArrange();
            var service = new DataWarehouse(logger.Object);

            // Act
            foreach (var model in models)
                service.TryAdd(model as IModel);

            var report = service.GetReport();

            // Assert
            Assert.Equal(clientCount, report.ClientCount);
            Assert.Equal(salesmanCount, report.SalesmanCount);
            Assert.Equal(mostExpensiveSaleId, report.MostExpensiveSaleId);
            Assert.Equal(worstSalesman, report.WorstSalesman);
        }

        [Theory(DisplayName = "Get Parallel Valid Report")]
        [MemberData(nameof(GetValidReportdMockData))]
        public void TestParallelGetReportValid(int clientCount, int salesmanCount, int mostExpensiveSaleId, string worstSalesman,
            IEnumerable<object> models)
        {
            // Arrange
            DefaultArrange();
            var service = new DataWarehouse(logger.Object);

            // Act
            Parallel.ForEach(models, (model) => { service.TryAdd(model as IModel); });

            var report = service.GetReport();

            // Assert
            Assert.Equal(clientCount, report.ClientCount);
            Assert.Equal(salesmanCount, report.SalesmanCount);
            Assert.Equal(mostExpensiveSaleId, report.MostExpensiveSaleId);
            Assert.Equal(worstSalesman, report.WorstSalesman);
        }
    }
}

