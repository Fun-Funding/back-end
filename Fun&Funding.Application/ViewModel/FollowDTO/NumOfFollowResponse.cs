using Fun_Funding.Application.ViewModel.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.FollowDTO
{
    public class NumOfFollowResponse
    {
        public int TotalFollow { get; set; }
        public List<UserInfoResponse>? Users { get; set; } 
    }
}
