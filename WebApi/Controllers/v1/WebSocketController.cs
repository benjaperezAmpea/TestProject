using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using System.Threading.Tasks;
using WebApi.Middlewares;

namespace WebApi.Controllers.v1
{
    /// <summary>
    /// WebSocketController
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class WebSocketController : ControllerBase
    {
        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await WebSocketHandlerMiddleware.HandleWebSocketAsync(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}
