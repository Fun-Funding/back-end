using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.UserDTO
{
    public class UserUpdateRequest
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public UserStatus? UserStatus { get; set; }
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? PhoneNumber { get; set; }
    }
}
