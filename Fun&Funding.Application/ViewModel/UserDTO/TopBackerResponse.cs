using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.UserDTO
{
    public class TopBackerResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string AvatarURL { get; set; } = string.Empty;
        public decimal TotalDonation { get; set; }
    }
}
