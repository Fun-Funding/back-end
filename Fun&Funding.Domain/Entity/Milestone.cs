using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Domain.Entity
{
    public class Milestone : BaseEntity
    {
        public string MilestoneName { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public int MilestoneOrder { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DisbursementPercentage { get; set; }
        public DateTime UpdateDate { get; set; }
        public MilestoneType? MilestoneType { get; set; }
        public int Version { get; set; }
    
        public virtual ICollection<ProjectMilestone> ProjectMilestones { get; set; }   
        public virtual ICollection<Requirement> Requirements { get; set; }
    }
}
