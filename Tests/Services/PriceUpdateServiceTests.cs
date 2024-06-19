using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Middlewares;
using WebApi.Services;
using WebApi.Services.AlphaVantageService;
namespace Tests.Services
{
    [TestFixture]
    public class PriceUpdateServiceTests
    {
        private Mock<IAlphaVantageService> _alphaVantageServiceMock;
        private Mock<ILogger<PriceUpdateService>> _loggerMock;
        private PriceUpdateService _service;
        private List<string> _testInstruments;
        private List<PriceUpdate> _testPriceUpdates;

        [SetUp]
        public void Setup()
        {
            _alphaVantageServiceMock = new Mock<IAlphaVantageService>();
            _loggerMock = new Mock<ILogger<PriceUpdateService>>();

            _testInstruments = new List<string> { "EURUSD", "USDJPY", "BTCUSD" };
            _testPriceUpdates = _testInstruments.Select(instr => new PriceUpdate
            {
                Instrument = instr,
                Price = 1.0m,
                Timestamp = System.DateTime.Now
            }).ToList();

            _alphaVantageServiceMock.Setup(s => s.GetAvailableInstruments()).Returns(_testInstruments);
            _alphaVantageServiceMock.Setup(s => s.GetPriceAsync(It.IsAny<string>()))
                .ReturnsAsync((string symbol) => _testPriceUpdates.First(p => p.Instrument == symbol));

            _service = new PriceUpdateService(_alphaVantageServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task ExecuteAsync_FetchesAndBroadcastsPriceUpdates()
        {
            // Arrange
            var stoppingToken = new CancellationTokenSource();
            stoppingToken.CancelAfter(2000); // To stop after 2 seconds to test the loop

            // Act
            await _service.StartAsync(stoppingToken.Token);

            // Assert
            _alphaVantageServiceMock.Verify(s => s.GetAvailableInstruments(), Times.AtLeastOnce);
            _alphaVantageServiceMock.Verify(s => s.GetPriceAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Test]
        public async Task FetchPriceUpdatesAsync_ReturnsCorrectPriceUpdates()
        {
            // Act
            var result = await _service.FetchPriceUpdatesAsync(_testInstruments);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_testInstruments.Count, result.Count);
            foreach (var instrument in _testInstruments)
            {
                Assert.IsTrue(result.Any(r => r.Instrument == instrument));
            }
        }
    }
}
