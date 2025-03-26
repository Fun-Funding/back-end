using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class BackgroundProcessService : IBackgroundProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private DateTime present = DateTime.Now;
        private IEmailService _emailService;
        public BackgroundProcessService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task UpdateFundingStatus()
        {
            try
            {
                // Rewrite the query to avoid unsupported operations.
                List<FundingProject> projects = _unitOfWork.FundingProjectRepository
                    .GetQueryable()
                    .Include(p => p.ProjectMilestones)
                    .Where(p => (p.Status == ProjectStatus.Approved ||
                        p.Status == ProjectStatus.Pending ||
                        p.Status == ProjectStatus.Processing) && p.IsDeleted == false)
                    .ToList();

                foreach (var project in projects)
                {
                    bool statusChanged = false;
                    // If project stil present and end date has already pass
                    if (project.Status == ProjectStatus.Processing && project.EndDate.Date <= present.Date)
                    {
                        if (project.Balance < project.Target)
                        {
                            project.Status = ProjectStatus.Failed;
                            await RefundFundingBackers(project.Id);
                            statusChanged = true;
                        }else if (project.Balance >= project.Target)
                        {
                            project.Status = ProjectStatus.FundedSuccessful;
                            if (project.ProjectMilestones != null)
                            {
                                foreach (var pm in project.ProjectMilestones)
                                {
                                    pm.Status = ProjectMilestoneStatus.Completed;
                                }
                            }
                           
                            statusChanged = true;
                        }
                       
                    }
                    // If admin has already approved project and start date reach today's date
                    else if (project.Status == ProjectStatus.Approved)
                    {
                        if (project.StartDate.Date == present.Date)
                        {
                            project.Status = ProjectStatus.Processing;
                            statusChanged = true;
                        }
                    }
                    
                    else if (project.Status == ProjectStatus.Pending && project.StartDate <= present)
                    {
                        project.Status = ProjectStatus.Rejected;
                        statusChanged = true;
                    }
                    if (statusChanged)
                    {
                        _unitOfWork.FundingProjectRepository.Update(project);
                    }
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateProjectMilestoneStatus()
        {
            try
            {
                var projectMilestones = _unitOfWork.ProjectMilestoneRepository
                    .GetQueryable()
                    .Include(pm => pm.Milestone)
                    .Where(p => (p.Status == ProjectMilestoneStatus.Pending
                    || p.Status == ProjectMilestoneStatus.Processing
                    || p.Status == ProjectMilestoneStatus.Warning) && p.Milestone.MilestoneType == MilestoneType.Disbursement).ToList();
                foreach (var projectMilestone in projectMilestones)
                {
                    bool statusChanged = false;
                    User owner = GetUserByProjectMilestoneId(projectMilestone.FundingProjectId);
                    FundingProject project = GetProject(projectMilestone.FundingProjectId);
                    if (projectMilestone.Status == ProjectMilestoneStatus.Processing)
                    {
                        if ((projectMilestone.EndDate.Date - present.Date).TotalDays == 7)
                        {
                            await _emailService.SendMilestoneAsync(owner.Email, project.Name, projectMilestone.Milestone.MilestoneName, owner.FullName, null, 7, present, EmailType.MilestoneReminder);
                        }
                        if ((projectMilestone.EndDate.Date - present.Date).TotalDays <= 0)
                        {
                            projectMilestone.Status = ProjectMilestoneStatus.Submitted;
                            await _emailService.SendMilestoneAsync(owner.Email, project.Name, projectMilestone.Milestone.MilestoneName, owner.FullName, "Submitted for review", null, present, EmailType.MilestoneExpired);
                            statusChanged = true;
                        }
                       

                    }
                    else if (projectMilestone.Status == ProjectMilestoneStatus.Warning)
                    {
                        if ((projectMilestone.EndDate.Date - present.Date).TotalDays == 7)
                        {
                            await _emailService.SendMilestoneAsync(owner.Email, project.Name, projectMilestone.Milestone.MilestoneName, owner.FullName, null, 7, present, EmailType.MilestoneReminder);
                        }else if ((projectMilestone.EndDate.Date - present).TotalDays <= 0)
                        {
                            projectMilestone.Status = ProjectMilestoneStatus.Resubmitted;
                            await _emailService.SendMilestoneAsync(owner.Email, project.Name, projectMilestone.Milestone.MilestoneName, owner.FullName, "Failed", null, present, EmailType.MilestoneExpired);
                            statusChanged = true;
                        }
                    }

                    if (statusChanged)
                    {
                        _unitOfWork.ProjectMilestoneRepository.Update(projectMilestone);
                    }
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public User GetUserByProjectMilestoneId(Guid projectId)
        {
            FundingProject project = _unitOfWork.FundingProjectRepository.GetQueryable()
                .Include(p => p.User).FirstOrDefault(p => p.Id == projectId);   
            if (project == null)
            {
                return null;
            }
            User owner = _unitOfWork.UserRepository.GetById(project.UserId);
            if(owner == null)
            {
                return null;
            }
            return owner;
        }

        public FundingProject GetProject(Guid projectId)
        {
            FundingProject project = _unitOfWork.FundingProjectRepository.GetQueryable()
               .Include(p => p.User).FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return null;
            }
            return project;
        }

        public async Task RefundFundingBackers(Guid id)
        {
            var packageBackers = await _unitOfWork.PackageBackerRepository.GetQueryable()
                    .Include(pb => pb.User)
                    .Where(pb => pb.Package.ProjectId == id)
                    .ToListAsync();
            var project = _unitOfWork.FundingProjectRepository.GetQueryable()
                .Include(p => p.Wallet).
                FirstOrDefault(p => p.Id == id);
            decimal totalContribution = packageBackers.Sum(pb => pb.DonateAmount);
            decimal refundableAmount = project.Wallet.Balance;

            // Refund backers proportionally based on their contribution
            foreach (var backer in packageBackers)
            {
                decimal backerContributionPercentage = backer.DonateAmount / totalContribution;
                decimal backerRefundAmount = backerContributionPercentage * refundableAmount;

                // Add the refund amount to the backer's wallet
                var backerWallet = await _unitOfWork.WalletRepository.GetQueryable().FirstOrDefaultAsync(w => w.Backer.Id == backer.UserId);
                if (backerWallet == null)
                {
                    backerWallet = new Wallet
                    {
                        Backer = backer.User,
                        Balance = 0,
                        BankAccountId = Guid.NewGuid() // Replace with actual logic for associating a bank account
                    };
                    await _unitOfWork.WalletRepository.AddAsync(backerWallet);
                }

                backerWallet.Balance += backerRefundAmount;

                // Log the transaction
                var transaction = new Transaction
                {
                    WalletId = backerWallet.Id,
                    TotalAmount = backerRefundAmount,
                    TransactionType = TransactionTypes.FundingRefund,
                    CreatedDate = DateTime.Now,
                    Description = "Refund to backers",
                    PackageId = backer.PackageId
                };
                await _unitOfWork.TransactionRepository.AddAsync(transaction);
            }
        }

       
    }
}
