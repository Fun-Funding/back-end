using Fun_Funding.Application.ViewModel.BankAccountDTO;
using Fun_Funding.Application.ViewModel.FundingFileDTO;
using Fun_Funding.Application.ViewModel.PackageDTO;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.FundingProjectDTO
{
    public class FundingProjectUpdateRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;

        public BankAccountUpdateRequest? BankAccount { get; set; }
        //public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<PackageUpdateRequest> Packages { get; set; }
        //public IFormFile ThumbnailFile { get; set; }

        //public List<IFormFile> Stories {  get; set; }

        public List<FundingFileUpdateRequest>? FundingFiles { get; set; }

        public List<FundingFileResponse>? ExistedFile { get; set; }

    }
}
