using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using WebApi.Services.AlphaVantageService;
using WebApi.Services.HttpClientWrapper;


namespace Tests.Services
{

    [TestFixture]
    public class AlphaVantageServiceTests
    {
        private Mock<IHttpClientWrapper> _httpClientMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<AlphaVantageService>> _loggerMock;
        private AlphaVantageService _service;

        [SetUp]
        public void Setup()
        {
            _httpClientMock = new Mock<IHttpClientWrapper>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AlphaVantageService>>();
            _configurationMock.Setup(c => c["AlphaVantage:ApiKey"]).Returns("test-api-key");

            _service = new AlphaVantageService(_httpClientMock.Object, _configurationMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetPriceAsync_ValidSymbol_ReturnsPriceUpdate()
        {
            // Arrange
            var symbol = "AAPL";
            var responseContent = @"
            {
                ""Time Series (1min)"": {
                    ""2021-01-01 00:00:00"": {
                        ""1. open"": ""0.92"",
                        ""2. high"": ""1.12"",
                        ""3. low"": ""1.88"",
                        ""4. close"": ""1.0"",
                        ""5. volume"": ""1.0""
                    }
                }
            }";
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            _httpClientMock.Setup(client => client.GetStringAsync(It.IsAny<string>())).ReturnsAsync(responseContent);

            // Act
            var result = await _service.GetPriceAsync(symbol);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(symbol, result.Instrument);
            Assert.AreEqual(1.0m, result.Price);
        }

        [Test]
        public void GetAvailableInstruments_ReturnsListOfInstruments()
        {
            // Act
            var result = _service.GetAvailableInstruments();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<string>>(result);
            Assert.Contains("EURUSD", result);
            Assert.Contains("USDJPY", result);
            Assert.Contains("BTCUSD", result);
        }
    }
}
