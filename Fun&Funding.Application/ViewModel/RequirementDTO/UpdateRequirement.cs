using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.RequirementDTO
{
    public class UpdateRequirement
    {
        public Guid RequirementId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
