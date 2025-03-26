using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.RequirementDTO
{
    public class RequirementResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public int Order { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid MilestoneId { get; set; }
        public FixedRequirementStatus Status { get; set; }
        public bool IsDeleted { get; set; }

    }
}
