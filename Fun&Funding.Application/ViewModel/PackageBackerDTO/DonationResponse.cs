using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Application.ViewModel.PackageBackerDTO
{
    public class DonationResponse
    {
        public string UserName { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DonateAmount { get; set; }
        public string PackageName { get; set; }
        public DateTime CreateDate { get; set; }
        public PackageType Types { get; set; }
        // Computed property to return a string based on PackageType
        public string PackageTypeDescription
        {
            get
            {
                return Types switch
                {
                    PackageType.Free => "free package",
                    PackageType.FixedPackage => "fixed package",
                    _ => "Unknown package type"
                };
            }
        }
    }
}
