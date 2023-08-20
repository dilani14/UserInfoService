using UserInfoService.Core.Models;

namespace UserInfoService.Core.Interfaces
{
    public interface IUserInfoRepository
    {
        Task<List<UserInfo>> GetUserInfoAsync();
        Task<string> GetUserNameByIdAsync(int id);
        Task<int> AddUserInfoAsync(UserInfo userInfo);
        Task UpdateUserInfoAsync(UserInfo userInfo, int id);
        Task DeleteUserInfoAsync(int id);
        Task<bool> IsUserInfoExistsAsync(int id);
        Task<bool> IsNameExistsAsync(string name);
    }
}
