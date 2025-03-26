using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.TransactionDTO
{
    public class TransactionInfoResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Description { get; set; }
        public decimal TotalAmount { get; set; }
        public TransactionTypes TransactionType { get; set; }
        public Guid? PackageId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? CommissionFeeId { get; set; }

        public Guid? ProjectMilestoneId { get; set; }
        public string? MilestoneName { get; set; }
        public decimal? DisbursementPercentage { get; set; }
        public int MilestoneType { get; set; }

        public decimal? CommissionFee { get; set; }
    }
}
