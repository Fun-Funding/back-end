using Fun_Funding.Application.ViewModel.WalletDTO;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Application.ViewModel.UserDTO
{
    public class UserInfoResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public UserStatus? UserStatus { get; set; }
        public WalletInfoResponse? Wallet { get; set; }
    }
}
