using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity
{
    public class FundingFile : BaseEntity
    {
        public string Name { get; set; }
        public string URL { get; set; }       
        public FileType Filetype { get; set; }

        public Guid? FundingProjectId { get; set; }

    }
}
