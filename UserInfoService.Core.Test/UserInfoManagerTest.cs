using Xunit;
using Moq;
using FluentAssertions;
using UserInfoService.Core.Interfaces;
using UserInfoService.Core.Managers;
using UserInfoService.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using UserInfoService.Core.Exceptions;
using UserInfoService.Core.Dto;
using Microsoft.Extensions.Logging;

namespace UserInfoService.Core.Test
{
    public class UserInfoManagerTest
    {
        private readonly static string INVALID_NAME_ERR_MSG = "An entry with the identical name already exists. Please select a different name.";
        private readonly static string INVALID_ID_ERR_MSG = "The provided Id does not have associated IdentityData. Please provide a valid Id.";

        private readonly Mock<IUserInfoRepository> _mockRepository;
        private readonly Mock<ICacheManager<List<UserInfo>>> _mockCacheManager;
        private readonly Mock<ILogger<UserInfoManager>> _mockLogger;
        
        public UserInfoManagerTest()
        {
            _mockRepository =  new Mock<IUserInfoRepository>();
            _mockCacheManager = new Mock<ICacheManager<List<UserInfo>>>();
            _mockLogger = new Mock<ILogger<UserInfoManager>>();
        }

        #region GetUserInfo
        [Fact]
        public async Task GetUserInfo_CacheIsEmpty_CallsGetUserInfoAsync()
        {
            //Arrange
            List<UserInfo> userInfosCache = null;

            _mockRepository.Setup(m => m.GetUserInfoAsync()).ReturnsAsync(UserInfoList());
            _mockCacheManager.Setup(m => m.Get(It.IsAny<string>())).Returns(userInfosCache);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            //Act
            await manager.GetUserInfo();

            //Assert
            _mockRepository.Verify(m => m.GetUserInfoAsync(), Times.Once());
        }

        [Fact]
        public async Task GetUserInfo_CacheIsEmptyUserInfoExists_CallsSetToCache()
        {
            //Arrange
            List<UserInfo> userInfosCache = null;
            MemoryCacheEntryOptions options = GetOptions();

            _mockRepository.Setup(m => m.GetUserInfoAsync()).ReturnsAsync(UserInfoList());
           
            _mockCacheManager.Setup(m => m.Get(It.IsAny<string>())).Returns(userInfosCache);
            _mockCacheManager.Setup(m => 
                m.GenerateMemoryCacheEntryOptions(It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CacheItemPriority>()))
                .Returns(options);
            _mockCacheManager.Setup(m => m.Set(It.IsAny<string>(), It.IsAny<List<UserInfo>>(), It.IsAny<MemoryCacheEntryOptions>()));

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            //Act
            await manager.GetUserInfo();

            //Assert
            _mockCacheManager.Verify(m => m.Set(CacheKeys.USERINFO_LIST,
                It.IsAny<List<UserInfo>>(), options), Times.Once());
        }

        [Fact]
        public async Task GetUserInfo_CacheIsEmptyUserInfoNotExists_NeverCallsSetToCache()
        {
            //Arrange
            List<UserInfo>? userInfos = null;
            MemoryCacheEntryOptions options = GetOptions();

            _mockRepository.Setup(m => m.GetUserInfoAsync()).ReturnsAsync(new List<UserInfo>());

            _mockCacheManager.Setup(m => m.Get(It.IsAny<string>())).Returns(userInfos);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            //Act
            await manager.GetUserInfo();

            //Assert
            _mockCacheManager.Verify(m => m.Set(CacheKeys.USERINFO_LIST, It.IsAny<List<UserInfo>>(), options), Times.Never());
        }

        [Fact]
        public async Task GetUserInfo_CacheIsEmpty_ReturnsUserInfoFromRepository()
        {
            //Arrange
            List<UserInfo>? userInfos = null;

            _mockRepository.Setup(m => m.GetUserInfoAsync()).ReturnsAsync(UserInfoList());

            _mockCacheManager.Setup(m => m.Get(It.IsAny<string>())).Returns(userInfos);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            //Act
            List<UserInfo> actual = await manager.GetUserInfo();

            //Assert
            actual.Should().BeEquivalentTo(UserInfoList());
        }

        [Fact]
        public async Task GetUserInfo_CacheIsNotEmpty_NeverCallsGetUserInfoAsync()
        {
            //Arrange
            List<UserInfo> userInfoList = UserInfoList();

            _mockCacheManager.Setup(m => m.Get(It.IsAny<string>())).Returns(userInfoList);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            //Act
            await manager.GetUserInfo();

            //Assert
            _mockRepository.Verify(m => m.GetUserInfoAsync(), Times.Never());
        }

        [Fact]
        public async Task GetUserInfo_CacheIsNotEmpty_ReturnsUserInfoFromCache()
        {
            //Arrange
            _mockCacheManager.Setup(m => m.Get(It.IsAny<string>())).Returns(UserInfoList());

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            //Act
            List<UserInfo> actual = await manager.GetUserInfo();

            //Assert
            actual.Should().BeEquivalentTo(UserInfoList());
        }
        #endregion

        #region AddUserInfo
        [Fact]
        public async Task AddUserInfo_NameExists_ThrowsException()
        {
            // Arrange
            _mockRepository.Setup(m => m.IsNameExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            // Act
            Func<Task> action = async () => await manager.AddUserInfo(new Dto.AddOrUpdateUserInfoRequest());

            // Assert
            await action.Should().ThrowAsync<InValidRequestDataException>().WithMessage(INVALID_NAME_ERR_MSG);
        }

        [Fact]
        public async Task AddUserInfo_NameNotExists_CallsAddUserInfoAsync()
        {
            // Arrange
            int id = 1;
            AddOrUpdateUserInfoRequest request = GetAddOrUpdateRequest();

            _mockRepository.Setup(m => m.IsNameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockRepository.Setup(m => m.AddUserInfoAsync(It.IsAny<UserInfo>())).ReturnsAsync(id);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            // Act
            await manager.AddUserInfo(request);

            // Assert
            _mockRepository.Verify(m => m.AddUserInfoAsync(It.Is<UserInfo>(u => 
                u.Name == request.Name 
                && u.Address == request.Address)));
        }
        #endregion

        #region UpdateUserInfo
        [Fact]
        public async Task UpdateUserInfo_UserInfoNotExists_ThrowsException()
        {
            // Arrange
            int id = 1;
            AddOrUpdateUserInfoRequest request = GetAddOrUpdateRequest();

            _mockRepository.Setup(m => m.IsUserInfoExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            // Act
            Func<Task> action = async () => await manager.UpdateUserInfo(request, id);

            // Assert
            await action.Should().ThrowAsync<InValidRequestDataException>().WithMessage(INVALID_ID_ERR_MSG);
        }

        [Fact]
        public async Task UpdateUserInfo_UserInfoExistsUpdateNameToDifferentName_ThrowsException()
        {
            // Arrange
            int id = 1;
            AddOrUpdateUserInfoRequest request = GetAddOrUpdateRequest();

            _mockRepository.Setup(m => m.IsUserInfoExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockRepository.Setup(m => m.IsNameExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            // Act
            Func<Task> action = async () => await manager.UpdateUserInfo(request, id);

            // Assert
            await action.Should().ThrowAsync<InValidRequestDataException>().WithMessage(INVALID_NAME_ERR_MSG);
        }

        [Fact]
        public async Task UpdateUserInfo_UserInfoExistsUpdateNameToDifferentName_CallsUpdateUserInfoAsync()
        {
            // Arrange
            int id = 1;
            AddOrUpdateUserInfoRequest request = GetAddOrUpdateRequest();

            _mockRepository.Setup(m => m.IsUserInfoExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockRepository.Setup(m => m.IsNameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockRepository.Setup(m => m.UpdateUserInfoAsync(It.IsAny<UserInfo>(), It.IsAny<int>()));

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            // Act
            await manager.UpdateUserInfo(request, id);

            // Assert
            _mockRepository.Verify(m => m.UpdateUserInfoAsync(It.Is<UserInfo>(u =>
                u.Name == request.Name
                && u.Address == request.Address), id));
        }
        #endregion

        #region DeleteUserInfo
        [Fact]
        public async Task DeleteUserInfo_UserInfoNotExists_ThrowsException()
        {
            // Arrange
            int id = 1;

            _mockRepository.Setup(m => m.IsUserInfoExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            // Act
            Func<Task> action = async () => await manager.DeleteUserInfo(id);

            // Assert
            await action.Should().ThrowAsync<InValidRequestDataException>().WithMessage(INVALID_ID_ERR_MSG);
        }

        [Fact]
        public async Task DeleteUserInfo_UserInfoExists_CallsDeleteUserInfoAsync()
        {
            // Arrange
            int id = 1;

            _mockRepository.Setup(m => m.IsUserInfoExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            UserInfoManager manager = new UserInfoManager(_mockRepository.Object, _mockCacheManager.Object, _mockLogger.Object);

            // Act
            await manager.DeleteUserInfo(id);

            // Assert
            _mockRepository.Verify(m => m.DeleteUserInfoAsync(id), Times.Once);
        }
        #endregion


        #region Private Methods
        private List<UserInfo> UserInfoList()
        {
            List<UserInfo> userInfoList = new List<UserInfo>();
            userInfoList.Add(new UserInfo
            {
                Id = 1,
                Name = "ABC Company",
                Address = "12, Cambrige Street, Calgery, ON"
            });
            userInfoList.Add(new UserInfo
            {
                Id = 2,
                Name = "gtp LTD",
                Address = "789, Olive Street, Brampton, ON"
            });

            return userInfoList;
        }

        private MemoryCacheEntryOptions GetOptions()
        {
            return new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(1))
               .SetAbsoluteExpiration(TimeSpan.FromDays(1))
               .SetPriority(CacheItemPriority.Normal);
        }

        private AddOrUpdateUserInfoRequest GetAddOrUpdateRequest()
        {
            return new() { Name = "Omega Solutions", Address = "21, Fitzgibson St, London, ON" };
        }
        #endregion
    }
}