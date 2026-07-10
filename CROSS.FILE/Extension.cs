using Microsoft.Extensions.DependencyInjection;

namespace CROSS.FILE
{
    public static class Extension
    {

        public static IServiceCollection AddFileServer(this IServiceCollection services)
        {
            services.AddSingleton<IFileServer, FileServer>();
            return services;
        }
    }
}
