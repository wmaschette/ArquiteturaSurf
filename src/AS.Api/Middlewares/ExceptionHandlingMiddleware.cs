using AS.Api.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AS.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate Next;

        public ExceptionHandlingMiddleware(RequestDelegate next) => Next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await RequestLog(context);
                await Next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
                ErrorLog(ex);
            }
        }

        private async Task RequestLog(HttpContext context)
        {
            context.Request.EnableBuffering();
            var jsonBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
            context.Request.Body.Position = 0;
            Console.WriteLine($"TIPO: Request - VERBO: {context.Request.Method} - API: {context.Request.Path} - JSON: {jsonBody} - QUERYSTRING: {queryString}");
        }

        private static Task HandleException(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var resultJson = JsonConvert.SerializeObject(new { exception.Message });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(resultJson);
        }

        private static void ErrorLog(Exception ex)
        {
            Console.WriteLine($"TIPO: Erro - MENSAGEM: {ex.Message} - CLASSE: {ex.GetType().ToString()}");
        }
    }
}
