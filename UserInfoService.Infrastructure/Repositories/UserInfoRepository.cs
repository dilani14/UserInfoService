using UserInfoService.Core.Interfaces;
using UserInfoService.Core.Models;
using UserInfoService.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace UserInfoService.Infrastructure.Repositories
{
    public class UserInfoRepository : IUserInfoRepository
    {
        private readonly UserInfoDbContext _userInfoDbContext;

        public UserInfoRepository(UserInfoDbContext userInfoDbContext)
        {
            _userInfoDbContext = userInfoDbContext;
        }

        public async Task<List<UserInfo>> GetUserInfoAsync()
        {
            return await _userInfoDbContext.UserInfo.ToListAsync();
        }

        public async Task<UserInfo?> GetUserInfoByIdAsync(int id)
        {
            return await _userInfoDbContext.UserInfo.FindAsync(id);
        }

        public async Task<int> AddUserInfoAsync(UserInfo userInfo)
        {          
            _userInfoDbContext.UserInfo.Add(userInfo);
            await _userInfoDbContext.SaveChangesAsync();
            return userInfo.Id;
        }

        public async Task UpdateUserInfoAsync(UserInfo userInfo, int id)
        {
            UserInfo? data = await _userInfoDbContext.UserInfo.FindAsync(id);
            if (data != null)
            {
                data.Name = userInfo.Name;
                data.Address = userInfo.Address;

                await _userInfoDbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteUserInfoAsync(int id)
        {
            UserInfo? data = await _userInfoDbContext.UserInfo.FindAsync(id);

            if (data != null)
            {
                _userInfoDbContext.UserInfo.Remove(data);
                await _userInfoDbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> IsNameExistsAsync(string name)
        {
            UserInfo? data = await _userInfoDbContext.UserInfo.FirstOrDefaultAsync(data => data.Name == name);
            return data != null;
        }

        public async Task<bool> IsDifferentDataWithSameNameExistsAsync(string name, int id)
        {
            UserInfo? data = await _userInfoDbContext.UserInfo.FirstOrDefaultAsync(data => data.Name == name && data.Id != id);
            return data != null;
        }

    }
}
