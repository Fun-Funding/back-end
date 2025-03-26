using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.FundingFileDTO
{
    public class FundingFileUpdateRequest
    {
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IFormFile UrlFile { get; set; }
        public FileType Filetype { get; set; }

        public bool IsDeleted { get; set; }
    }
}
