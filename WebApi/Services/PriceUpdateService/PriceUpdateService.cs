using Domain.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Middlewares;
using WebApi.Services.AlphaVantageService;


namespace WebApi.Services
{
    public class PriceUpdateService : BackgroundService, IPriceUpdateService
    {
        private readonly IAlphaVantageService _alphaVantageService;
        private readonly ILogger<PriceUpdateService> _logger;

        public PriceUpdateService(IAlphaVantageService alphaVantageService, ILogger<PriceUpdateService> logger)
        {
            _alphaVantageService = alphaVantageService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ((IPriceUpdateService)this).ExecuteAsync(stoppingToken);
        }

        async Task IPriceUpdateService.ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PriceUpdateService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Fetching available instruments.");
                var instruments = _alphaVantageService.GetAvailableInstruments();
                _logger.LogInformation($"Fetched instruments: {string.Join(", ", instruments)}");

                _logger.LogInformation("Fetching price updates for instruments.");
                var priceUpdates = await FetchPriceUpdatesAsync(instruments);
                _logger.LogInformation("Broadcasting price updates.");
                await WebSocketHandlerMiddleware.BroadcastPriceUpdates(priceUpdates);

                _logger.LogInformation("Waiting for the next update cycle.");
                await Task.Delay(60000, stoppingToken); // 1 minute delay to not exceed the API limit
            }

            _logger.LogInformation("PriceUpdateService is stopping.");
        }

        public async Task<List<PriceUpdate>> FetchPriceUpdatesAsync(List<string> instruments)
        {
            var tasks = instruments.Select(instrument => _alphaVantageService.GetPriceAsync(instrument)).ToList();
            var priceUpdates = await Task.WhenAll(tasks);
            return priceUpdates.ToList();
        }
    }
}
