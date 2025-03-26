using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class ProjectMilestoneBacker : BaseEntity
    {
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Star { get; set; }
        public string Comment { get; set; }

        public Guid BackerId { get; set; }
        public User Backer { get; set; }
        public Guid ProjectMilestoneId {get; set;}
        public ProjectMilestone ProjectMilestone { get; set;}
    }
}
