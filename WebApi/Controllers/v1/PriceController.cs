using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using System.Threading.Tasks;
using WebApi.Middlewares;
using WebApi.Services.AlphaVantageService;

namespace WebApi.Controllers.v1
{
    /// <summary>
    /// PriceControlle
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class PriceController : ControllerBase
    {
        private readonly IAlphaVantageService _alphaVantageService;
        
        public PriceController(IAlphaVantageService alphaVantageService)
        {
            _alphaVantageService = alphaVantageService;
        }
        
        /// <summary>
        /// Get Story By Id and process it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<IActionResult> GetAvailableSymbols()
        {

            var result = _alphaVantageService.GetAvailableInstruments();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Get Story By Id and process it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetPriceBySymbol(string symbol)
        {
            var result = await _alphaVantageService.GetPriceAsync(symbol);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
