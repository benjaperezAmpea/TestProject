using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;


namespace WebApi.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlerMiddleware> logger;
        private readonly string jsonContentType = "application/json";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex) when (ex is ValidationException vEx)
            {
                logger.LogError(ex.ToString());
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = jsonContentType;

                var errorMessages = vEx.Errors
                    .Select(e => e.ErrorMessage);

                if (!errorMessages.Any())
                    errorMessages = errorMessages.Append(vEx.Message);

                await context.Response.WriteAsJsonAsync(CreateErrorFromMessages(nameof(ExceptionHandlerMiddleware),
                    errorMessages));
            }
            catch (Exception ex) when (ex is OperationCanceledException)
            {
                logger.LogDebug(ex.ToString());
                context.Response.StatusCode = StatusCodes.Status410Gone;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = jsonContentType;

                await context.Response.WriteAsJsonAsync(CreateErrorFromMessages(
                    nameof(ExceptionHandlerMiddleware),
                    "An error occurred on the server."));
            }
        }

        private static SerializableError CreateErrorFromMessages(string name, params string[] messages) =>
           CreateErrorFromMessages(name, messages.AsEnumerable());

        private static SerializableError CreateErrorFromMessages(string name, IEnumerable<string> messages) => new()
        {
            {
                "errors",
                new Dictionary<string, List<string>>()
                {
                    {
                        name,
                        messages.ToList()
                    }
                }
            }
        };
    }
}
