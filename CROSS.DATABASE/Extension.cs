namespace CROSS.DATABASE
{
    using Microsoft.Extensions.DependencyInjection;

    public static class Extension
    {
        public static IServiceCollection AddDataBase(this IServiceCollection services)
        {
            services.AddSingleton<IManagerDataBase, ManagerDataBase>();
            return services;
        }
    }
}
