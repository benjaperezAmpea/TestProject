using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Services.AlphaVantageService
{
    public interface IAlphaVantageService
    {
        Task<PriceUpdate> GetPriceAsync(string symbol);
        List<string> GetAvailableInstruments();
    }
}