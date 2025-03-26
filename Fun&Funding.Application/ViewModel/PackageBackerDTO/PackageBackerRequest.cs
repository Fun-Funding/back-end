using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.PackageBackerDTO
{
    public class PackageBackerRequest
    {
        public Guid UserId { get; set; }
        public Guid PackageId { get; set; }
        public decimal DonateAmount {  get; set; }
    }
}
