using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.BankAccountDTO;
using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class BankAccountService :IBankAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResultDTO<BankAccount>> GetBankAccountByWalletId(Guid id)
        {
            try
            {
                var result = await _unitOfWork.BankAccountRepository.GetAsync(x => x.Wallet.Id == id);
                if (result is null)
                    return ResultDTO<BankAccount>.Fail("not found");
                return ResultDTO<BankAccount>.Success(result, "Successfully found");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<string>> LinkUserBankAccount(BankAccountUpdateRequest bankRequest)
        {
            try
            {
                // param @id is walletId
                var wallet = await _unitOfWork.WalletRepository.GetAsync(w => w.Id == bankRequest.Id);
                if (wallet == null)
                    return ResultDTO<string>.Fail("Wallet not found!");

                var bankAccount = new BankAccount
                {
                    Id = new Guid(),
                    BankCode = bankRequest.BankCode,
                    BankNumber = bankRequest.BankNumber,
                };
                wallet.BankAccount = bankAccount;

                _unitOfWork.WalletRepository.Update(wallet);

                await _unitOfWork.CommitAsync();

                return ResultDTO<string>.Success("Link bank account successfully!");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
