using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
namespace CROSS.LOGGER
{

    public static class Extension
    {
        public static ILoggingBuilder AddLogger(this ILoggingBuilder logging, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
            logging.ClearProviders();
            logging.AddSerilog(Log.Logger);
            return logging;
        }
    }
}