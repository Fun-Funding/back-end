using Fun_Funding.Application.IService;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.TransactionDTO;
using Fun_Funding.Application.ExceptionHandler;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateTransactionAsync(
            decimal totalAmount,
            string description,
            TransactionTypes transactionType,
            Guid walletId,
            Guid? packageId,
            Guid? systemWalletId = null,
            Guid? commissionFeeId = null,
            Guid? orderDetailId = null,
            Guid? orderId = null
        )
        {
            var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(walletId) ?? throw new Exception("Wallet not found");
            var transaction = new Transaction
            {
                Description = description,
                TotalAmount = totalAmount,
                TransactionType = transactionType,
                PackageId = packageId,
                WalletId = walletId,
                //Wallet = wallet,
                SystemWalletId = systemWalletId,
                CommissionFeeId = commissionFeeId,
                OrderId = orderId,
                OrderDetailId = orderDetailId,
                CreatedDate = DateTime.Now,
            };

            await _unitOfWork.TransactionRepository.AddAsync(transaction);
            await _unitOfWork.CommitAsync();

        }

        public async Task<ResultDTO<List<TransactionInfoResponse>>> GetAllTransactionsByProjectId(Guid? projectId, TransactionFilter filter = TransactionFilter.All)
        {
            try
            {
                if (projectId == null || projectId == Guid.Empty)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project ID cannot be null or empty.");
                }

                // Get all transactions related to the specific project ID
                var transactions = filter == TransactionFilter.All ? await _unitOfWork.TransactionRepository.GetQueryable()
                    .Include(t => t.Wallet)
                    .Include(t => t.ProjectMilestone.Milestone)
                    .Include(t => t.CommissionFee)
                    .Where(t =>
                        (t.PackageId != null &&
                         _unitOfWork.PackageRepository.GetQueryable()
                             .Any(p => p.Id == t.PackageId && p.ProjectId == projectId)) // Check Package related to the project
                        ||
                        (t.ProjectMilestoneId != null &&
                         _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                             .Any(pm => pm.Id == t.ProjectMilestoneId && pm.FundingProjectId == projectId)) // Check Milestone related to the project
                        ||
                        (t.WalletId != null &&
                         _unitOfWork.WalletRepository.GetQueryable()
                             .Any(w => w.Id == t.WalletId && w.FundingProject.Id == projectId)) // Check Wallet related to the project
                    ).OrderByDescending(t => t.CreatedDate)
                    .ToListAsync() : await _unitOfWork.TransactionRepository.GetQueryable()
                    .Include(t => t.Wallet)
                    .Include(t => t.ProjectMilestone.Milestone)
                    .Include(t => t.CommissionFee)
                    .Where(t =>
                        
                        (t.ProjectMilestoneId != null &&
                         _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                             .Any(pm => pm.Id == t.ProjectMilestoneId && pm.FundingProjectId == projectId)) // Check Milestone related to the project
                        )
                    .OrderByDescending(t => t.CreatedDate)
                    .ToListAsync();

                if (transactions == null || transactions.Count == 0)
                {
                    return ResultDTO<List<TransactionInfoResponse>>.Fail("No transactions found for the specified project.");
                }

                // Map the transactions to TransactionInfoResponse
                var response = transactions.Select(transaction => new TransactionInfoResponse
                {
                    Id = transaction.Id,
                    CreatedDate = transaction.CreatedDate,
                    Description = transaction.Description,
                    TotalAmount = transaction.TotalAmount,
                    TransactionType = transaction.TransactionType,
                    PackageId = transaction.PackageId,
                    OrderId = transaction.OrderId,
                    CommissionFeeId = transaction.CommissionFeeId,
                    ProjectMilestoneId = transaction.ProjectMilestoneId,
                    MilestoneName = transaction.ProjectMilestone?.Milestone.MilestoneName,
                    DisbursementPercentage = transaction.ProjectMilestone?.Milestone.DisbursementPercentage / 2,
                    CommissionFee = transaction.CommissionFee?.Rate,

                }).ToList();

                return ResultDTO<List<TransactionInfoResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResultDTO<IEnumerable<object>>> GetAllTransactionsByMarketId(Guid? projectId)
        {
            try
            {
                var marketProject = _unitOfWork.MarketplaceRepository.GetQueryable()
                    .Include(m => m.Wallet).FirstOrDefault(w => w.Id == projectId);
                var transactions = _unitOfWork.TransactionRepository.GetQueryable().
                    Where(t => t.WalletId == marketProject.Wallet.Id).OrderByDescending(t => t.CreatedDate);
                var response = transactions.Select(t => new
                {
                    t.TransactionType,
                    t.CommissionFee,
                    t.Description,
                    t.TotalAmount,
                    t.CreatedDate
                }).ToList();
                return ResultDTO<IEnumerable<object>>.Success(response);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
