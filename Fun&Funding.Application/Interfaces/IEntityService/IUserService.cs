using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.UserDTO;
using Fun_Funding.Domain.Entity;

namespace Fun_Funding.Application.IService
{
    public interface IUserService
    {
        Task<ResultDTO<PaginatedResponse<UserInfoResponse>>> GetUsers(ListRequest request);
        Task<ResultDTO<UserInfoResponse>> GetUserInfo();
        Task<ResultDTO<UserInfoResponse>> GetUserInfoById(Guid id);
        Task<ResultDTO<string>> UpdateUser(UserUpdateRequest userUpdateRequest);
        Task<ResultDTO<UserInfoResponse>> ChangeUserStatus(Guid id);
        Task<ResultDTO<string>> ChangeUserPassword(UserChangePasswordRequest userChangePasswordRequest);
        Task<ResultDTO<string>> UploadUserAvatar(UserFileRequest userFileRequest);
        Task<ResultDTO<string>> CheckUserPassword(string password);
        Task<ResultDTO<string>> CheckUserRole(User user);
        Task<ResultDTO<List<TopBackerResponse>>> GetTop4Backer();
        Task<ResultDTO<decimal>> CountPlatformUsers();
        Task<ResultDTO<string>> GetUserRole(Guid id);
        Task<ResultDTO<UserInfoResponse>> CreateUser(UserCreateRequest request);
        Task<List<User>> GetUsersByRoleAsync(string roleName);
    }
}
