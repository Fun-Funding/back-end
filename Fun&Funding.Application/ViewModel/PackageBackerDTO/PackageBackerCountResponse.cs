using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.PackageBackerDTO
{
    public class PackageBackerCountResponse
    {
        public Guid PackageId { get; set; }
        public string PackageName { get; set; }
        public int Count { get; set; }
    }
}
