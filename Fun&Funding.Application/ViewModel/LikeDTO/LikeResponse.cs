using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.LikeDTO
{
    public class LikeResponse
    {
        public Guid ProjectId { get; set; }
        public Guid UserID { get; set; }
    }
}
