using Fun_Funding.Domain.Constrain;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.UserDTO
{
    public class UserCreateRequest
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public Gender? Gender { get; set; }
        public UserFileRequest? File { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public UserStatus UserStatus { get; set; }
        public int Role { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
