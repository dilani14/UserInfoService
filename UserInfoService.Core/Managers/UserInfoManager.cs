using UserInfoService.Core.Interfaces;
using UserInfoService.Core.Models;
using System.Net;
using UserInfoService.Core.Exceptions;
using UserInfoService.Core.Dto;
using Microsoft.Extensions.Logging;

namespace UserInfoService.Core.Managers
{
    public class UserInfoManager
    {
        private readonly static string INVALID_NAME_ERR_MSG = "An entry with the identical name already exists. Please select a different name.";
        private readonly static string INVALID_ID_ERR_MSG = "The provided Id does not have associated IdentityData. Please provide a valid Id.";

        private readonly IUserInfoRepository _userInfoRepository;
        private readonly ICacheManager<List<UserInfo>> _cacheManager;
        private readonly ILogger<UserInfoManager> _logger;

        public UserInfoManager(IUserInfoRepository userInfoRepository, ICacheManager<List<UserInfo>> cacheManager, ILogger<UserInfoManager> logger)
        {
            _userInfoRepository = userInfoRepository;
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public async Task<List<UserInfo>> GetUserInfo()
        {
            _logger.LogInformation("Info : Start GetUserInfo");

            List<UserInfo>? userInfoList = new();
            userInfoList = _cacheManager.GetFromCache(CacheKeys.USERINFO_LIST);
            if (userInfoList != null)
            {
                _logger.LogInformation("Info : End GetUserInfo, Return from cache");
                return userInfoList;
            }

            userInfoList = await _userInfoRepository.GetUserInfoAsync();

            if (userInfoList.Count > 0)
            {
                var options = _cacheManager.GenerateMemoryCacheEntryOptions(TimeSpan.FromMinutes(1), TimeSpan.FromDays(1));
                _cacheManager.SetToCache(CacheKeys.USERINFO_LIST, userInfoList, options);
            }

            _logger.LogInformation("Info : End GetUserInfo, Return from repository");
            return userInfoList.ToList();
        }

        public async Task<int> AddUserInfo(AddOrUpdateUserInfoRequest request)
        {
            _logger.LogInformation("Info : Start AddUserInfo");

            if (await _userInfoRepository.IsNameExistsAsync(request.Name))
            {
                throw new InValidRequestDataException(INVALID_NAME_ERR_MSG, (int)HttpStatusCode.BadRequest);
            }

            UserInfo userInfo = new() { Name = request.Name, Address = request.Address };
            int id = await _userInfoRepository.AddUserInfoAsync(userInfo);

            _cacheManager.RemoveFromCache(CacheKeys.USERINFO_LIST);

            _logger.LogInformation("Info : End AddUserInfo");

            return id;
        }

        public async Task UpdateUserInfo(AddOrUpdateUserInfoRequest request, int id)
        {
            _logger.LogInformation("Info : Start UpdateUserInfo"); 

            if (!await _userInfoRepository.IsUserInfoExistsAsync(id))
            {
                throw new InValidRequestDataException(INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            string currentName = await _userInfoRepository.GetUserNameByIdAsync(id);
            // Check whether name field is getting updated
            if (currentName != request.Name)
            {
                if (await _userInfoRepository.IsNameExistsAsync(request.Name))
                {
                    throw new InValidRequestDataException(INVALID_NAME_ERR_MSG, (int)HttpStatusCode.BadRequest);
                }
            }

            UserInfo userInfo = new() { Name = request.Name, Address = request.Address };
            await _userInfoRepository.UpdateUserInfoAsync(userInfo, id);

            _cacheManager.RemoveFromCache(CacheKeys.USERINFO_LIST);

            _logger.LogInformation("Info : End UpdateUserInfo");
        }

        public async Task DeleteUserInfo(int id)
        {
            _logger.LogInformation("Info : Start DeleteUserInfo");

            if (!await _userInfoRepository.IsUserInfoExistsAsync(id))
            {
                throw new InValidRequestDataException(INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            await _userInfoRepository.DeleteUserInfoAsync(id);

            _cacheManager.RemoveFromCache(CacheKeys.USERINFO_LIST);

            _logger.LogInformation("Info : End DeleteUserInfo");
        }
    }
}
