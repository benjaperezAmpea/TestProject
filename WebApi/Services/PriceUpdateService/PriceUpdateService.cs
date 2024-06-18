using Domain.Models;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Middlewares;
using WebApi.Services.AlphaVantageService;

namespace WebApi.Services
{
    public class PriceUpdateService : BackgroundService
    {
        private readonly IAlphaVantageService _alphaVantageService;

        public PriceUpdateService(IHttpClientFactory httpClientFactory, IAlphaVantageService alphaVantageService)
        {
            _alphaVantageService = alphaVantageService;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var priceUpdate = await FetchPriceUpdateAsync();
                await WebSocketHandlerMiddleware.BroadcastPriceUpdate(priceUpdate);
                await Task.Delay(50000, stoppingToken);
            }
        }

        private async Task<PriceUpdate> FetchPriceUpdateAsync()
        {
            var response = await _alphaVantageService.GetPriceAsync("AAPL");
            return response;
        }
    }
}
