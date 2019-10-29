using DataAnalyzerModels;
using DataAnalyzerServices;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace DataAnalyzerXUnitTest
{
    public class ProcessorTest
    {
        private Mock<ILogger<IProcessor>> logger;

        private void DefaultArrange()
        {
            logger = new Mock<ILogger<IProcessor>>(MockBehavior.Loose);
        }

        [Theory(DisplayName = "Process valid lines")]
        [InlineData("QWERTY", default(IModel))]
        [InlineData("001�1234567891234�Pedro�50000", typeof(Salesman))]
        [InlineData("001�1234567891234�Jo�o Silva�50000.90", typeof(Salesman))]
        [InlineData("001�1234567891234�Pedro�50000.999999", typeof(Salesman))]
        [InlineData("002�2345675434544345�Jose da Silva�Rural", typeof(Client))]
        [InlineData("002�2345675434544345�Jose da Silva�Rural e N�o rural", typeof(Client))]
        [InlineData("003�10�[1-10-100.50]�Pedro", typeof(Sale))]
        [InlineData("003�8�[1-10-100,2-30-2.50,3-40-3.10]�Pedro", typeof(Sale))]
        public void TestProcessLine(string line, Type resulType)
        {
            // Arrange
            DefaultArrange();
            var service = new Processor(logger.Object);

            // Act
            var response = service.ProcessLine(line);

            // Assert
            if (response == null)
                Assert.Null(resulType);
            else
                Assert.Equal(resulType, response.GetType());
        }

        [Theory(DisplayName = "Process invalid line separators")]
        [InlineData("QWERTY")]
        [InlineData("001���")]
        [InlineData("001�1234567891234�Pedro�50000")]
        [InlineData("0011234567891234�Pedro�50000")]
        [InlineData("001�1234567891234Pedro�50000")]
        [InlineData("001�1234567891234�Pedro50000")]
        [InlineData("001�1234567891234�Pedro�50000�")]
        [InlineData("002�2345675434544345�Jose�da�Silva�Rura�l")]
        public void TestProcessLineInvalidLineSeparators(string line)
        {
            // Arrange
            DefaultArrange();
            var service = new Processor(logger.Object);

            // Act
            var response = service.ProcessLine(line);

            // Assert
            Assert.Null(response);
        }

        [Theory(DisplayName = "Process invalid names")]
        [InlineData("001�1234567891234�Pa�oca�50000")]
        [InlineData("002�2345675434544345�Pa�oca�Rural")]
        [InlineData("002�2345675434544345�Pedro�Pa�oca")]
        [InlineData("003�10�[1-10-100.50]�Pa�oca")]
        public void TestProcessInvalidNames(string line)
        {
            // Arrange
            DefaultArrange();
            var service = new Processor(logger.Object);

            // Act
            var response = service.ProcessLine(line);

            // Assert
            Assert.Null(response);
        }

        [Theory(DisplayName = "Process invalid items list")]
        [InlineData("003�8�[[1-10-100,2-30-2.50,3-40-3.10]]�Pedro")]
        [InlineData("003�8�{1-10-100,2-30-2.50,3-40-3.10}�Pedro")]
        [InlineData("003�8�[1-10-1002-30-2.503-40-3.10]�Pedro")]
        [InlineData("003�8�[1-10-1002-30]�Pedro")]
        [InlineData("003�8�[---]�Pedro")]
        [InlineData("003�8�[------]�Pedro")]
        [InlineData("003�8�[]�Pedro")]
        [InlineData("003�8�[1-50-]�Pedro")]
        [InlineData("003�8�[-50-100.50]�Pedro")]
        [InlineData("003�8�[1.99-50.99-100.50]�Pedro")]
        [InlineData("003�10�1-10-100.50�Pedro")]
        public void TestProcessInvalidItemsList(string line)
        {
            // Arrange
            DefaultArrange();
            var service = new Processor(logger.Object);

            // Act
            var response = service.ProcessLine(line);

            // Assert
            Assert.Null(response);
        }

        [Theory(DisplayName = "Process unknown codes")]
        [InlineData("1�1234567891234�Pedro�50000")]
        [InlineData("004�1234567891234�Pedro�50000")]
        [InlineData("000�1234567891234�Jo�o Silva�50000.90")]
        public void TestProcessInvalidCodes(string line)
        {
            // Arrange
            DefaultArrange();
            var service = new Processor(logger.Object);

            // Act
            var response = service.ProcessLine(line);

            // Assert
            Assert.Null(response);
        }
    }
}
