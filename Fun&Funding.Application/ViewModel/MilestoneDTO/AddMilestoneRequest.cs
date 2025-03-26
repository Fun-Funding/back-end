using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.MilestoneDTO
{
    public class AddMilestoneRequest
    {
        public string MilestoneName { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public int MilestoneOrder { get; set; }
        public decimal DisbursementPercentage { get; set; }
    }
}
