using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class RewardItem : BaseEntity
    {       
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }

        
    }
}
