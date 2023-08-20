using Microsoft.Extensions.DependencyInjection;
using UserInfoService.Core.Managers;

namespace UserInfoService.Core
{
    public static class CoreDependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddTransient<UserInfoManager>();
            return services;
        }
    }
}
