using CROSS.AUTH.IAM.Services;
using CROSS.AUTH.TOKEN;
using Microsoft.Extensions.DependencyInjection;

namespace CROSS.AUTH.IAM
{
    public static class Extension
    {
        public static IServiceCollection AddAuthIam(this IServiceCollection services)
        {
            services.AddTokenService();
            services.AddSingleton<IAuthIamService, AuthIamService>();
            return services;
        }
    }
}