using Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace WebApi.Services
{
    public interface IPriceUpdateService
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
        Task<List<PriceUpdate>> FetchPriceUpdatesAsync(List<string> instruments);
    }
}
