using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.PackageBackerDTO
{
    public class ProjectBackerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public decimal DonateAmount { get; set; }
    }
}
