using UserInfoService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace UserInfoService.Infrastructure.DbContexts
{
    public class UserInfoDbContext : DbContext
    {
        public UserInfoDbContext(DbContextOptions<UserInfoDbContext> options) : base(options)
        {
        }

        public DbSet<UserInfo> UserInfo { get; set; }
    }
}
