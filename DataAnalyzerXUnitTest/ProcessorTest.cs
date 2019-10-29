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
        [InlineData("001Á1234567891234ÁPedroÁ50000", typeof(Salesman))]
        [InlineData("001Á1234567891234ÁJo„o SilvaÁ50000.90", typeof(Salesman))]
        [InlineData("001Á1234567891234ÁPedroÁ50000.999999", typeof(Salesman))]
        [InlineData("002Á2345675434544345ÁJose da SilvaÁRural", typeof(Client))]
        [InlineData("002Á2345675434544345ÁJose da SilvaÁRural e N„o rural", typeof(Client))]
        [InlineData("003Á10Á[1-10-100.50]ÁPedro", typeof(Sale))]
        [InlineData("003Á8Á[1-10-100,2-30-2.50,3-40-3.10]ÁPedro", typeof(Sale))]
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
        [InlineData("001ÁÁÁ")]
        [InlineData("001«1234567891234«Pedro«50000")]
        [InlineData("0011234567891234ÁPedroÁ50000")]
        [InlineData("001Á1234567891234PedroÁ50000")]
        [InlineData("001Á1234567891234ÁPedro50000")]
        [InlineData("001Á1234567891234ÁPedroÁ50000Á")]
        [InlineData("002Á2345675434544345ÁJoseÁdaÁSilvaÁRuraÁl")]
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
        [InlineData("001Á1234567891234ÁPaÁocaÁ50000")]
        [InlineData("002Á2345675434544345ÁPaÁocaÁRural")]
        [InlineData("002Á2345675434544345ÁPedroÁPaÁoca")]
        [InlineData("003Á10Á[1-10-100.50]ÁPaÁoca")]
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
        [InlineData("003Á8Á[[1-10-100,2-30-2.50,3-40-3.10]]ÁPedro")]
        [InlineData("003Á8Á{1-10-100,2-30-2.50,3-40-3.10}ÁPedro")]
        [InlineData("003Á8Á[1-10-1002-30-2.503-40-3.10]ÁPedro")]
        [InlineData("003Á8Á[1-10-1002-30]ÁPedro")]
        [InlineData("003Á8Á[---]ÁPedro")]
        [InlineData("003Á8Á[------]ÁPedro")]
        [InlineData("003Á8Á[]ÁPedro")]
        [InlineData("003Á8Á[1-50-]ÁPedro")]
        [InlineData("003Á8Á[-50-100.50]ÁPedro")]
        [InlineData("003Á8Á[1.99-50.99-100.50]ÁPedro")]
        [InlineData("003Á10Á1-10-100.50ÁPedro")]
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
        [InlineData("1Á1234567891234ÁPedroÁ50000")]
        [InlineData("004Á1234567891234ÁPedroÁ50000")]
        [InlineData("000Á1234567891234ÁJo„o SilvaÁ50000.90")]
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
