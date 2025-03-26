using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class Category : BaseEntity
    {
        public string? Name { get; set; }
        public virtual ICollection<FundingProject> Projects { get; set; }
    }
}
