using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.BankAccountDTO;
using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Interfaces.IEntityService
{
    public interface IBankAccountService
    {
        public Task<ResultDTO<BankAccount>> GetBankAccountByWalletId(Guid id);
        public Task<ResultDTO<string>> LinkUserBankAccount(BankAccountUpdateRequest bankRequest);
    }
}
