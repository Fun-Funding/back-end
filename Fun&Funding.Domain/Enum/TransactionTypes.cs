using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Enum
{
    public enum TransactionTypes
    {
        PackageDonation,
        AddWalletMoney,
        WithdrawWalletMoney,
        FundingWithdraw, // or cashout
        CommissionFee,
        FundingRefund, // case funding project fails
        FundingPurchase,
        OrderPurchase,
        MarketplaceWithdraw,
        MilestoneFirstHalf,
        MilestoneSecondHalf,
        WithdrawRefund,
        WithdrawCancel,
        WithdrawFundingMilestone
    }
}
