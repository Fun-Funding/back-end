using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.ProjectRequirementFileDTO
{
    public class ProjectRequirementFileUpdateRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public FileType Filetype { get; set; }
        public bool IsDeleted { get; set; } 
    }
}
