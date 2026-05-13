using Microsoft.AspNetCore.Http;
using Serilog;
using System.Net;

namespace CROSS.EXCEPTION_HANDLER
{
    public abstract class ExceptionHandler
    {
        private readonly RequestDelegate _next;

        public abstract (HttpStatusCode code, string message) GetResponse(CustomException exception);

        public ExceptionHandler(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await this._next(httpContext);
            }
            catch (CustomException exception)
            {
                Log.Error(exception, exception.Message);
                HttpResponse response = httpContext.Response;
                response.ContentType = "application/json";
                //string message = JsonConvert.SerializeObject(new
                //{
                //    Data = null as string,
                //    Meta = new
                //    {
                //        exception.Message,
                //        exception.StatusCode,
                //        Success = false
                //    },
                //});
                (HttpStatusCode status, string message) = GetResponse(exception);
                response.StatusCode = (int)status;
                await response.WriteAsync(message);
            }
        }
    }
}