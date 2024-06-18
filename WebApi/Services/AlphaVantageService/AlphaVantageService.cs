using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;
using Domain.Models.Responses;
using System.Text.Json;
using System.Linq;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace WebApi.Services.AlphaVantageService
{
    public class AlphaVantageService : IAlphaVantageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<AlphaVantageService> _logger;
        public AlphaVantageService(HttpClient httpClient, IConfiguration configuration, ILogger<AlphaVantageService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AlphaVantage:ApiKey"];
            _logger = logger;
        }

        public async Task<PriceUpdate> GetPriceAsync(string symbol)
        {
            _logger.LogInformation($"Get Price information for symbol {symbol}");
            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=1min&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var priceResponse = JsonSerializer.Deserialize<AlphaVantageResponse>(response);

#if DEBUG
            if (priceResponse == null || priceResponse.TimeSeries == null)
            {
                priceResponse = new AlphaVantageResponse { TimeSeries = new Dictionary<string, TimeSeriesEntry>() };
                priceResponse.TimeSeries.Add("2021-01-01 00:00:00", new TimeSeriesEntry
                {
                    Open = "0.92",
                    High = "1.12",
                    Low = "1.88",
                    Close = "1.0",
                    Volume = "1.0",
                    Timestamp = DateTime.Now
                });
            }
#endif


            if (priceResponse == null || priceResponse.TimeSeries == null )
            {
                return null;
            }

            var latestEntry = priceResponse.TimeSeries.Values.FirstOrDefault();
            if (latestEntry == null)
            {
                return null;
            }

            return new PriceUpdate
            {
                Instrument = symbol,
                Price = decimal.Parse(latestEntry.Close),
                Timestamp = latestEntry.Timestamp
            };
        }

        public List<string> GetAvailableInstruments()
        {
            return new List<string> { "EURUSD", "USDJPY", "BTCUSD" };
        }
    }

    public class AlphaVantageResponse
    {
        [JsonPropertyName("Time Series (1min)")]
        public Dictionary<string, TimeSeriesEntry> TimeSeries { get; set; }
    }

    public class TimeSeriesEntry
    {
        [JsonPropertyName("1. open")]
        public string Open { get; set; }

        [JsonPropertyName("2. high")]
        public string High { get; set; }

        [JsonPropertyName("3. low")]
        public string Low { get; set; }

        [JsonPropertyName("4. close")]
        public string Close { get; set; }

        [JsonPropertyName("5. volume")]
        public string Volume { get; set; }

        [JsonIgnore]
        public DateTime Timestamp { get; set; }
    }
}
