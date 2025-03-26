using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.UserDTO;
using Fun_Funding.Domain.Constrain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fun_Funding.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private IAzureService _storageService;
        public UserController(IAzureService storageService
            , IUserService userService)
        {
            _storageService = storageService;
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] ListRequest request)
        {
            var response = await _userService.GetUsers(request);
            return Ok(response);
        }
        [HttpGet("info")]
        public async Task<IActionResult> GetUserInfo()
        {
            var response = await _userService.GetUserInfo();
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserById([FromRoute] Guid id)
        {
            var response = await _userService.GetUserInfoById(id);
            return Ok(response);
        }
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<ActionResult> CreateUser([FromForm] UserCreateRequest userCreateRequest)
        {
            var response = await _userService.CreateUser(userCreateRequest);
            return Ok(response);
        }
        [HttpPatch("info")]
        [Authorize]
        public async Task<ActionResult> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            var response = await _userService.UpdateUser(userUpdateRequest);
            return Ok(response);
        }
        [HttpPatch("status/{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ChangeUserStatus([FromRoute] Guid id)
        {
            var response = await _userService.ChangeUserStatus(id);
            return Ok(response);
        }
        [HttpPatch("password")]
        [Authorize]
        public async Task<IActionResult> ChangeUserPassword(UserChangePasswordRequest userChangePasswordRequest)
        {
            var response = await _userService.ChangeUserPassword(userChangePasswordRequest);
            return Ok(response);
        }
        [HttpGet("password")]
        [Authorize]
        public async Task<IActionResult>CheckUserPassword(string password)
        {
            var response = await _userService.CheckUserPassword(password);
            return Ok(response);
        }
        [HttpPatch("avatar")]
        [Authorize]
        public async Task<IActionResult> UploadUserAvatar([FromForm] UserFileRequest userFileRequest)
        {
            var response = await _userService.UploadUserAvatar(userFileRequest);
            return Ok(response);
        }
        [HttpGet("top4")]
        public async Task<IActionResult> GetTop4Backer()
        {
            var response = await _userService.GetTop4Backer();
            return Ok(response);
        }
        [HttpGet("number-of-users")]
        public async Task<IActionResult> CountPlatformUsers()
        {
            var response = await _userService.CountPlatformUsers();
            return Ok(response);
        }
        [HttpGet("user-role/{id}")]
        public async Task<IActionResult> GetUserRole([FromRoute] Guid id)
        {
            var response = await _userService.GetUserRole(id);
            return Ok(response);
        }
    }
}
