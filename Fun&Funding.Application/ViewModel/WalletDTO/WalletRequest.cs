using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.WalletDTO
{
    public class WalletRequest
    {
        [Required]
        public Guid WalletId { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        public decimal Balance { get; set; }
    }
}
