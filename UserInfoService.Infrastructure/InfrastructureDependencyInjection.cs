using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserInfoService.Core.Interfaces;
using UserInfoService.Infrastructure.DbContexts;
using UserInfoService.Infrastructure.Repositories;

namespace UserInfoService.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddDbContext<UserInfoDbContext>(options => options.UseInMemoryDatabase("UserInfo"));
            services.AddTransient<IUserInfoRepository, UserInfoRepository>();
            return services;
        }
    }
}
