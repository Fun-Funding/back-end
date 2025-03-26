using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.WalletDTO;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITransactionService _transactionService;
        private readonly ClaimsPrincipal _claimsPrincipal;
        public WalletService(IUnitOfWork unitOfWork, IMapper mapper
            , IHttpContextAccessor httpContextAccessor, ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _transactionService = transactionService;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
        }

        public async Task<ResultDTO<WalletInfoResponse>> AddMoneyToWallet(WalletRequest walletRequest)
        {
            try
            {
                var wallet = await _unitOfWork.WalletRepository.GetQueryable()
                    .Include(w => w.Transactions)
                    .Include(w => w.WithdrawRequests)
                    .FirstOrDefaultAsync(w => w.Id == walletRequest.WalletId)
                        ?? throw new Exception("Wallet not found!");

                if (walletRequest.Balance < 2000)
                    return ResultDTO<WalletInfoResponse>.Fail("Invalid amount!");

                wallet.Balance += walletRequest.Balance;
                _unitOfWork.WalletRepository.Update(wallet);

                // add transaction
                await _transactionService.CreateTransactionAsync(
                    totalAmount: walletRequest.Balance,
                    description: "Add money to wallet",
                    transactionType: TransactionTypes.AddWalletMoney,
                    walletId: walletRequest.WalletId
                );

                await _unitOfWork.CommitAsync();

                var walletResponse = _mapper.Map<WalletInfoResponse>(wallet);

                return ResultDTO<WalletInfoResponse>.Success(walletResponse, $"{walletRequest.Balance} is successfully added to your wallet!");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<ResultDTO<WalletInfoResponse>> GetMarketplaceProject(Guid marketplaceProjectId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResultDTO<WalletInfoResponse>> GetWalletByUser()
        {
            try
            {
                if (_claimsPrincipal == null || !_claimsPrincipal.Identity.IsAuthenticated)
                {
                    throw new ExceptionError((int)HttpStatusCode.Unauthorized, "User not authenticated.");
                }
                var userEmailClaims = _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (userEmailClaims == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found.");
                }
                var userEmail = userEmailClaims.Value;
                var wallet = await _unitOfWork.WalletRepository.GetQueryable()
                    .Include(w => w.Transactions)
                    .Include(w => w.WithdrawRequests)
                    .FirstOrDefaultAsync(w => w.Backer.Email == userEmail)
                        ?? throw new Exception("Wallet not found!");
                var walletResponse = _mapper.Map<WalletInfoResponse>(wallet);
                return ResultDTO<WalletInfoResponse>.Success(walletResponse);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }
    }
}
