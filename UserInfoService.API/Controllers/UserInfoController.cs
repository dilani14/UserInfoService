using UserInfoService.Core.Managers;
using UserInfoService.Core.Dto;
using Microsoft.AspNetCore.Mvc;
using UserInfoService.Core.Models;

namespace IdentityDataService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly UserInfoManager _userInfoManager;

        public UserInfoController(UserInfoManager userInfoManager)
        {
            _userInfoManager = userInfoManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetIdentityData()
        {
            List<UserInfo> data = await _userInfoManager.GetUserInfo();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddIdentityData([FromBody] AddOrUpdateUserInfoRequest request)
        {
            int id = await _userInfoManager.AddUserInfo(request);

            return Created($"/api/identityData/{id}", id);
        }

        [Route("{id}"), HttpPut]
        public async Task<IActionResult> UpdateIdentityData([FromRoute] int id, [FromBody] AddOrUpdateUserInfoRequest request)
        {
            await _userInfoManager.UpdateUserInfo(request, id);

            return NoContent();
        }

        [Route("{id}"), HttpDelete]
        public async Task<IActionResult> DeleteIdentityData([FromRoute] int id)
        {
            await _userInfoManager.DeleteUserInfo(id);

            return NoContent();
        }
    }
}
