using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;

namespace CROSS.EXCEPTION_HANDLER
{
    public class ExceptionHandlerMiddleware : ExceptionHandler
    {
        public ExceptionHandlerMiddleware(RequestDelegate next)
            : base(next)
        {
        }

        public override (HttpStatusCode code, string message) GetResponse(CustomException exception)
        {
            return (HttpStatusCode.OK, JsonConvert.SerializeObject(new 
            {
                Data = null as string,
                Meta = new 
                {
                    exception.MessageLog,
                    exception.StatusCode,
                    Success = false
                },
            }));
        }
    }
}