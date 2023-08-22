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

        private const int initialRequestCount = 1;
        private const int maxRequestCount = 1;
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(initialRequestCount, maxRequestCount);

        public UserInfoManager(IUserInfoRepository userInfoRepository, ICacheManager<List<UserInfo>> cacheManager, ILogger<UserInfoManager> logger)
        {
            _userInfoRepository = userInfoRepository;
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public async Task<List<UserInfo>> GetUserInfo()
        {
            _logger.LogInformation("Start - Fetching the list of users");

            List<UserInfo>? userInfoList = _cacheManager.GetFromCache(CacheKeys.USERINFO_LIST);
            if (userInfoList != null)
            {
                _logger.LogInformation("End - User list Return from cache");
                return userInfoList;
            }

            userInfoList = await _userInfoRepository.GetUserInfoAsync();

            if (userInfoList.Count > 0)
            {
                var options = _cacheManager.GenerateMemoryCacheEntryOptions(TimeSpan.FromMinutes(1), TimeSpan.FromDays(1));
                _cacheManager.SetToCache(CacheKeys.USERINFO_LIST, userInfoList, options);
            }

            _logger.LogInformation("End - User list Return from database");
            return userInfoList.ToList();
        }

        public async Task<int> AddUserInfo(AddOrUpdateUserInfoRequest request)
        {
            await semaphoreSlim.WaitAsync();

            _logger.LogInformation("Start - Adding a user");

            try
            {
                if (await _userInfoRepository.IsNameExistsAsync(request.Name))
                {
                    throw new InValidRequestDataException(INVALID_NAME_ERR_MSG, (int)HttpStatusCode.BadRequest);
                }

                UserInfo userInfo = new() { Name = request.Name, Address = request.Address };
                int id = await _userInfoRepository.AddUserInfoAsync(userInfo);

                _cacheManager.RemoveFromCache(CacheKeys.USERINFO_LIST);

                _logger.LogInformation("End - Adding a user");

                return id;
            }
            finally
            {
                semaphoreSlim.Release();
            }

        }

        public async Task UpdateUserInfo(AddOrUpdateUserInfoRequest request, int id)
        {
            _logger.LogInformation("Start - Updating a user");

            if (!await _userInfoRepository.IsUserInfoExistsAsync(id))
            {
                throw new InValidRequestDataException(INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            await semaphoreSlim.WaitAsync();

            try
            {
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

                _logger.LogInformation("End - Updating a user");
            } 
            finally 
            { 
                semaphoreSlim.Release();
            }
            
        }

        public async Task DeleteUserInfo(int id)
        {
            _logger.LogInformation("Start - Deleteing a user");

            if (!await _userInfoRepository.IsUserInfoExistsAsync(id))
            {
                throw new InValidRequestDataException(INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            await _userInfoRepository.DeleteUserInfoAsync(id);

            _cacheManager.RemoveFromCache(CacheKeys.USERINFO_LIST);

            _logger.LogInformation("End - Deleteing a user");
        }
    }
}
