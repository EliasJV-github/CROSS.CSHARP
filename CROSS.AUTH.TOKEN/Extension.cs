using Microsoft.Extensions.DependencyInjection;

namespace CROSS.AUTH.TOKEN
{
    public static class Extension
    {
        public static IServiceCollection AddTokenService(this IServiceCollection services)
        {
            services.AddSingleton<ITokenService, TokenService>();
            return services;
        }
    }
}