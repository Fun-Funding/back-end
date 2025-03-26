using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.WalletDTO;

namespace Fun_Funding.Application.IService
{
    public interface IWalletService
    {
        Task<ResultDTO<WalletInfoResponse>> GetWalletByUser();
        Task<ResultDTO<WalletInfoResponse>> AddMoneyToWallet(WalletRequest walletRequest);
        Task<ResultDTO<WalletInfoResponse>> GetMarketplaceProject(Guid marketplaceProjectId);
    }
}
