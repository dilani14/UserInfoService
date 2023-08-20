using UserInfoService.Core.Interfaces;
using UserInfoService.Core.Models;
using System.Net;
using UserInfoService.Core.Exceptions;

namespace UserInfoService.Core.Managers
{
    public class UserInfoManager
    {
        private readonly static string INVALID_NAME_ERR_MSG = "An entry with the identical name already exists. Please select a different name.";
        private readonly static string INVALID_ID_ERR_MSG = "The provided Id does not have associated IdentityData. Please provide a valid Id.";

        private readonly IUserInfoRepository _userInfoRepository;

        public UserInfoManager(IUserInfoRepository userInfoRepository) 
        {
            _userInfoRepository = userInfoRepository;
        }

        public async Task<List<UserInfo>> GetUserInfo()
        {
           return await _userInfoRepository.GetUserInfoAsync();
        }

        public async Task<int> AddUserInfo(AddOrUpdateUserInfoRequest request)
        {
            if (await _userInfoRepository.IsNameExistsAsync(request.Name))
            {
                throw new InValidRequestDataException(INVALID_NAME_ERR_MSG, (int)HttpStatusCode.BadRequest);
            }

            UserInfo userInfo = new() { Name = request.Name, Address = request.Address };
            int id = await _userInfoRepository.AddUserInfoAsync(userInfo);

            return id;
        }

        public async Task UpdateUserInfo(AddOrUpdateUserInfoRequest request, int id)
        {            
            if (await _userInfoRepository.GetUserInfoByIdAsync(id) == null)
            {
                throw new InValidRequestDataException(INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            if (await _userInfoRepository.IsDifferentDataWithSameNameExistsAsync(request.Name, id))
            {
                throw new InValidRequestDataException(INVALID_NAME_ERR_MSG, (int)HttpStatusCode.BadRequest);
            }

            UserInfo userInfo = new() { Id = id, Name = request.Name, Address = request.Address };
            await _userInfoRepository.UpdateUserInfoAsync(userInfo, id);
        }

        public async Task DeleteUserInfo(int id)
        {
            if (await _userInfoRepository.GetUserInfoByIdAsync(id) == null)
            {
                throw new InValidRequestDataException(INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            await _userInfoRepository.DeleteUserInfoAsync(id);
        }
    }
}
