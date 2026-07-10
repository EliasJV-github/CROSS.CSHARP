using Microsoft.Extensions.DependencyInjection;

namespace CROSS.APIREST
{
    public static class Extension
    {
        public static IServiceCollection AddApiRest(this IServiceCollection services)
        {
            services.AddSingleton<IApiRest, ApiRest>();
            return services;
        }
    }
}
    