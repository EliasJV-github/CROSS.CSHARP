public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
            {
                _logger.LogError(ex, "Unhandled exception intercepted by ExceptionMiddleware");
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started — exception could not be masked. TraceId: {TraceId}",
                context.TraceIdentifier);
            return;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json";

        var (httpStatus, errorCode, message) = ResolveError(ex);
        context.Response.StatusCode = httpStatus;

        var wrapper = new WrapperResult<object?>
        {
            Data = null,
            Meta = new Meta
            {
                StatusCode = errorCode,
                Success = false,
                Message = message
            }
        };

        var json = JsonSerializer.Serialize(wrapper, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static (int httpStatus, string errorCode, string message) ResolveError(Exception ex) => ex switch
    {
        ArgumentNullException e => (400, "01", e.Message),
        ArgumentException e => (400, "02", e.Message),
        UnauthorizedAccessException e => (401, "03", e.Message),
        KeyNotFoundException e => (404, "04", e.Message),
        NotSupportedException e => (405, "05", e.Message),
        TimeoutException e => (408, "06", e.Message),
        InvalidOperationException e => (409, "07", e.Message),
        NotImplementedException e => (501, "08", e.Message),
        _ => (500, "99", "An unexpected error occurred.")
    };
}