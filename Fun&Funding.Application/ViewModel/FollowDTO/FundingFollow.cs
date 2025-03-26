using Fun_Funding.Application.ViewModel.FundingProjectDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.FollowDTO
{
    public class FundingFollow
    {
        public int FollowCount { get; set; }
        public List<FundingProjectResponse>? fundingProjectResponses { get; set; }
    }
}
