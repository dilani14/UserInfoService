using UserInfoService.Core.Models;

namespace UserInfoService.Core.Interfaces
{
    public interface IUserInfoRepository
    {
        Task<List<UserInfo>> GetUserInfoAsync();
        Task<int> AddUserInfoAsync(UserInfo userInfo);
        Task UpdateUserInfoAsync(UserInfo userInfo, int id);
        Task DeleteUserInfoAsync(int id);
    }
}
