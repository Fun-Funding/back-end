using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.AuthenticationDTO
{
    public class NewPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Token is required.")]
        public string? Token { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Password must be at least 7 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{7,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
        public string? NewPassword { get; set; }
    }
}
