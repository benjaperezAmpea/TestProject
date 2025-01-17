﻿using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Domain.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WebApi.Middlewares
{
    /// <summary>
    /// WebSocketMiddleware
    /// </summary>
    public class WebSocketHandlerMiddleware
    {
        private readonly ILogger<WebSocketHandlerMiddleware> _logger;
        private static readonly List<WebSocket> _sockets = new();
        private readonly RequestDelegate _next;

        /// <summary>
        /// WebSocketMiddleware Constructor
        /// </summary>
        /// <param name="next"></param>
        public WebSocketHandlerMiddleware(RequestDelegate next, ILogger<WebSocketHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                _logger.LogInformation("WebSocket request accepted.");
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _sockets.Add(webSocket);
                _logger.LogInformation("WebSocket connection established.");

                await HandleWebSocketAsync(webSocket);
            }
            else
            {
                await _next(context);
            }
        }

        /// <summary>
        /// HandleWebSocketAsync
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public static async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            _sockets.Remove(webSocket);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        /// <summary>
        /// Broadcast Prices Updates
        /// </summary>
        /// <param name="priceUpdates"></param>
        /// <returns></returns>
        public static async Task BroadcastPriceUpdates(List<PriceUpdate> priceUpdates)
        {
            foreach (var priceUpdate in priceUpdates)
            {
                var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(priceUpdate));
                var tasks = _sockets.Select(socket => socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None));
                await Task.WhenAll(tasks);

            }
        }
    }
}
