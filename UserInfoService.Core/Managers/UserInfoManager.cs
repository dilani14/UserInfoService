using UserInfoService.Core.Interfaces;
using UserInfoService.Core.Models;
using System.Net;
using UserInfoService.Core.Exceptions;
using UserInfoService.Core.Dto;
using Microsoft.Extensions.Logging;
using UserInfoService.Core.Helpers;

namespace UserInfoService.Core.Managers
{
    public class UserInfoManager
    {
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
            _logger.LogInformation("Start Fetching the list of users");

            List<UserInfo>? userInfoList = _cacheManager.Get(CacheKeys.USERINFO_LIST);
            if (userInfoList != null)
            {
                _logger.LogInformation("End User list Return from cache");
                return userInfoList;
            }

            userInfoList = await _userInfoRepository.GetUserInfoAsync();

            if (userInfoList.Count > 0)
            {
                var options = _cacheManager.GenerateMemoryCacheEntryOptions(TimeSpan.FromMinutes(1), TimeSpan.FromDays(1));
                _cacheManager.Set(CacheKeys.USERINFO_LIST, userInfoList, options);
            }

            _logger.LogInformation("End User list Return from database");
            return userInfoList.ToList();
        }

        public async Task<int> AddUserInfo(AddOrUpdateUserInfoRequest request)
        {

            _logger.LogInformation($"Start Adding a user with name {request.Name}");

            await semaphoreSlim.WaitAsync();
            _logger.LogTrace($"Lock obtained for Adding a user with name {request.Name}");

            try
            {
                if (await _userInfoRepository.IsNameExistsAsync(request.Name))
                {
                    throw new InValidRequestDataException(ErrorMsg.INVALID_NAME_ERR_MSG, (int)HttpStatusCode.BadRequest);
                }

                UserInfo userInfo = ObjectMapper.Mapper.Map<UserInfo>(request);
                int id = await _userInfoRepository.AddUserInfoAsync(userInfo);

                _cacheManager.Remove(CacheKeys.USERINFO_LIST);

                _logger.LogInformation($"End Adding a user with name {request.Name}");

                return id;
            }
            finally
            {
                semaphoreSlim.Release();
                _logger.LogTrace($"Lock released for Adding a user with name {request.Name}");
            }

        }

        public async Task UpdateUserInfo(AddOrUpdateUserInfoRequest request, int id)
        {
            _logger.LogInformation($"Start - Updating User Id - {id}");

            if (!await _userInfoRepository.IsUserInfoExistsAsync(id))
            {
                throw new InValidRequestDataException(ErrorMsg.INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            await semaphoreSlim.WaitAsync();
            _logger.LogTrace($"Lock obtained for Updating User Id - {id}");

            try
            {
                string currentName = await _userInfoRepository.GetUserNameByIdAsync(id);
                // Check whether name field is getting updated
                if (currentName != request.Name)
                {
                    if (await _userInfoRepository.IsNameExistsAsync(request.Name))
                    {
                        throw new InValidRequestDataException(ErrorMsg.INVALID_NAME_ERR_MSG, (int)HttpStatusCode.BadRequest);
                    }
                }

                UserInfo userInfo = ObjectMapper.Mapper.Map<UserInfo>(request);
                await _userInfoRepository.UpdateUserInfoAsync(userInfo, id);

                _cacheManager.Remove(CacheKeys.USERINFO_LIST);

                _logger.LogInformation($"End Updating User Id - {id}");
            } 
            finally 
            { 
                semaphoreSlim.Release();
                _logger.LogTrace($"Lock released for Updating User Id - {id}");
            }
            
        }

        public async Task DeleteUserInfo(int id)
        {
            _logger.LogInformation($"Start Deleteing User Id - {id}");

            if (!await _userInfoRepository.IsUserInfoExistsAsync(id))
            {
                throw new InValidRequestDataException(ErrorMsg.INVALID_ID_ERR_MSG, (int)HttpStatusCode.NotFound);
            }

            await _userInfoRepository.DeleteUserInfoAsync(id);

            _cacheManager.Remove(CacheKeys.USERINFO_LIST);

            _logger.LogInformation($"End Deleteing User Id - {id}");
        }
    }
}
