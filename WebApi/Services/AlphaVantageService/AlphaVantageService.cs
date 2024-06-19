using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebApi.Services.HttpClientWrapper;

namespace WebApi.Services.AlphaVantageService
{
    public class AlphaVantageService : IAlphaVantageService
    {
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly string _apiKey;
        private readonly ILogger<AlphaVantageService> _logger;
        public AlphaVantageService(IHttpClientWrapper httpClientWrapper, IConfiguration configuration, ILogger<AlphaVantageService> logger)
        {
            _httpClientWrapper = httpClientWrapper;
            _apiKey = configuration["AlphaVantage:ApiKey"];
            _logger = logger;
        }

        public async Task<PriceUpdate> GetPriceAsync(string symbol)
        {
            _logger.LogInformation($"Fetching price information for symbol: {symbol}");
            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=1min&apikey={_apiKey}";

            var response = await _httpClientWrapper.GetStringAsync(url);
            _logger.LogDebug($"Received response from AlphaVantage for symbol {symbol}: {response}");
            var priceResponse = JsonSerializer.Deserialize<AlphaVantageResponse>(response);

            if (priceResponse == null || priceResponse.TimeSeries == null)
            {
                _logger.LogWarning($"Price information for symbol {symbol} not found or empty response.");
            }

#if DEBUG
            if (priceResponse == null || priceResponse.TimeSeries == null)
            {
                _logger.LogDebug("Using debug fallback data for price response.");
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

            var latestEntry = priceResponse.TimeSeries.Values.FirstOrDefault();
            if (latestEntry == null)
            {
                _logger.LogWarning($"No entries found in the time series for symbol {symbol}.");
                return null;
            }

            _logger.LogInformation($"Successfully fetched latest price information for symbol {symbol}: {latestEntry.Close}");
            return new PriceUpdate
            {
                Instrument = symbol,
                Price = decimal.Parse(latestEntry.Close, CultureInfo.InvariantCulture),
                Timestamp = latestEntry.Timestamp
            };
        }

        /// <summary>
        /// Fetches a list of available instruments.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableInstruments()
        {
            _logger.LogInformation("Fetching list of available instruments.");
            var instruments = new List<string> { "EURUSD", "USDJPY", "BTCUSD" };
            _logger.LogInformation($"Available instruments: {string.Join(", ", instruments)}");
            return instruments;
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

