using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.FundingProjectDTO;
using Fun_Funding.Application.ViewModel.MilestoneDTO;
using Fun_Funding.Application.ViewModel.PackageBackerDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneBackerDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class ProjectMilestoneService : IProjectMilestoneService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private int maxExpireDay = 15;
        private int maxMilestoneExtend = 10;
        private ITransactionService _transactionService;
        private int lastMilestoneOrder = 4;
        private DateTime present = DateTime.Now;
        public ProjectMilestoneService(IUnitOfWork unitOfWork, IMapper mapper, ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _transactionService = transactionService;
        }

        public async Task<ResultDTO<ProjectMilestoneResponse>> CreateProjectMilestoneRequest(ProjectMilestoneRequest request)
        {
            try
            {
                FundingProject project = _unitOfWork.FundingProjectRepository
                    .GetQueryable().Include(p => p.ProjectMilestones)
                    .ThenInclude(pm => pm.Milestone).FirstOrDefault(p => p.Id == request.FundingProjectId);
                if (project == null)
                {
                    return ResultDTO<ProjectMilestoneResponse>.Fail("Project not found", 404);
                }
                //case project not funded successfully
                if (project.Status != ProjectStatus.FundedSuccessful)
                {
                    return ResultDTO<ProjectMilestoneResponse>.Fail("Project is not funded successfully", 500);
                }


                Milestone requestMilestone = _unitOfWork.MilestoneRepository
                    .GetQueryable().Include(m => m.Requirements)
                    .FirstOrDefault(m => m.Id == request.MilestoneId);

                // Check if this milestone has already been added to the project
                bool milestoneExists = project.ProjectMilestones
                    .Any(pm => pm.Milestone.Id == requestMilestone.Id);

                if (milestoneExists)
                {
                    return ResultDTO<ProjectMilestoneResponse>.Fail("This milestone has already been added to the project.", 400);
                }
                //case request first
                if (requestMilestone.MilestoneOrder == 1)
                {
                    if (present >= project.EndDate)
                    {
                        if ((present.Date - project.EndDate.Date).TotalDays > maxExpireDay)
                        {
                            return ResultDTO<ProjectMilestoneResponse>.Fail("The milestone must begin within 15 days after the project's funding period ends.", 500);
                        }
                    }
                    else
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "First milestone requested date must be after the date project funded successfully");
                    }

                }


                var checkValidateMilstone = CanCreateProjectMilestone(project, requestMilestone.MilestoneOrder, requestMilestone.CreatedDate);
                if (checkValidateMilstone != null)
                {
                    return ResultDTO<ProjectMilestoneResponse>.Fail(checkValidateMilstone, 500);
                }
                ProjectMilestone projectMilestone = new ProjectMilestone
                {
                    EndDate = DateTime.Now.AddDays(requestMilestone.Duration),
                    Status = ProjectMilestoneStatus.Pending,
                    MilestoneId = request.MilestoneId,
                    FundingProjectId = project.Id,
                    Title = request.Title,
                    IsDeleted = false,
                    CreatedDate = present
                };

                if (!String.IsNullOrEmpty(request.Title) && !String.IsNullOrEmpty(request.Introduction))
                {
                    projectMilestone.Introduction = request.Introduction;
                    projectMilestone.CreatedDate = present;
                }

                await _unitOfWork.ProjectMilestoneRepository.AddAsync(projectMilestone);
                _unitOfWork.Commit();
                return GetProjectMilestoneRequest(projectMilestone.Id).Result;

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

        public async Task<ResultDTO<ProjectMilestoneResponse>> GetProjectMilestoneRequest(Guid id)
        {
            try
            {
                ProjectMilestone projectMilestone = _unitOfWork.ProjectMilestoneRepository
                    .GetQueryable()
                    .Include(pm => pm.Milestone)
                    .ThenInclude(pmr => pmr.Requirements)
                    .Include(pm => pm.ProjectMilestoneRequirements)
                    .ThenInclude(pmr => pmr.Requirement)
                    .Include(pm => pm.ProjectMilestoneRequirements)
                    .ThenInclude(pmr => pmr.RequirementFiles)
                    .FirstOrDefault(pm => pm.Id == id);
                if (projectMilestone == null)
                {
                    return ResultDTO<ProjectMilestoneResponse>.Fail("Not found", 404);
                }
                ProjectMilestoneResponse result = _mapper.Map<ProjectMilestoneResponse>(projectMilestone);
                return ResultDTO<ProjectMilestoneResponse>.Success(result);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new Exception(ex.Message);
            }
        }

        public string CanCreateProjectMilestone(FundingProject project, int requestedMilestoneOrder, DateTime createdDate)
        {
            // Get all the project milestones ordered by MilestoneOrder
            var projectMilestones = project.ProjectMilestones
                //.Where(pm => pm.Milestone.MilestoneType == MilestoneType.Disbursement)
                .OrderBy(pm => pm.Milestone.MilestoneOrder)
                .ToList();
            if (projectMilestones != null || projectMilestones.Count != 0)
            {
                // Check if the requested milestone order is valid
                if (requestedMilestoneOrder > projectMilestones.Count + 1)
                    return "Requested milestone order is greater than the next available milestone"; // Requested milestone order is greater than the next available milestone

                // Check the status of the previous milestones
                for (int i = 0; i < requestedMilestoneOrder - 1; i++)
                {
                    var previousMilestone = projectMilestones[i];
                    if (previousMilestone.Status != ProjectMilestoneStatus.Completed)
                        return "The previous milestones are not completed";
                }
                if (requestedMilestoneOrder > 1)
                {
                    var previousMilestone = projectMilestones[requestedMilestoneOrder - 2];
                    if ((createdDate - previousMilestone.EndDate).TotalDays > maxMilestoneExtend)
                    {
                        return $"Requested days between each milestone must be within {maxMilestoneExtend} days";
                    }
                }

            }
            return null;
        }

        public string CanUpdateProjectMilestone(FundingProject project, int requestedMilestoneOrder)
        {
            // Get all the project milestones ordered by MilestoneOrder
            var projectMilestones = project.ProjectMilestones
                .OrderBy(pm => pm.Milestone.MilestoneOrder)
                .ToList();
            if (projectMilestones != null || projectMilestones.Count != 0)
            {
                // Check if the requested milestone order is valid
                if (requestedMilestoneOrder > projectMilestones.Count + 1)
                    return "Requested milestone order is greater than the next available milestone"; // Requested milestone order is greater than the next available milestone

                // Check the status of the previous milestones
                for (int i = 0; i < requestedMilestoneOrder - 1; i++)
                {
                    var previousMilestone = projectMilestones[i];
                    if (previousMilestone.Status != ProjectMilestoneStatus.Completed)
                        return "The previous milestones are not completed";
                }

            }
            return null;
        }

        public async Task<ResultDTO<List<ProjectMilestoneResponse>>> GetAllProjectMilestone()
        {
            try
            {
                var pmList = await _unitOfWork.ProjectMilestoneRepository
                    .GetQueryable()
                    .Include(pmb => pmb.Milestone)
                        .ThenInclude(pm => pm.Requirements)
                    .Include(pmb => pmb.FundingProject)
                    .ToListAsync();

                var responseList = new List<ProjectMilestoneResponse>();

                foreach (var item in pmList)
                {
                    responseList.Add(_mapper.Map<ProjectMilestoneResponse>(item));
                }

                return ResultDTO<List<ProjectMilestoneResponse>>.Success(responseList);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<string>> UpdateProjectMilestoneStatus(ProjectMilestoneStatusUpdateRequest request)
        {
            try
            {
                var projectMilestone = await _unitOfWork.ProjectMilestoneRepository
                    .GetQueryable()
                    .Include(pm => pm.Milestone)
                    .FirstOrDefaultAsync(pm => pm.Id == request.ProjectMilestoneId);
                if (projectMilestone == null) return ResultDTO<string>.Fail("The requested project milestone is not found!");

                var fundingProject = _unitOfWork.FundingProjectRepository
                    .GetQueryable()
                    .Include(p => p.Wallet)
                    .Include(p => p.ProjectMilestones)
                    .ThenInclude(pm => pm.Milestone)
                    .FirstOrDefault(p => p.Id == projectMilestone.FundingProjectId);

                if (fundingProject == null)
                {
                    throw new Exception("Corresponding funding project not found!");
                }



                // check project milestone current status
                // ...
                var pendingStatusList = new List<ProjectMilestoneStatus>() { ProjectMilestoneStatus.Processing };
                var processingStatusList = new List<ProjectMilestoneStatus>() { ProjectMilestoneStatus.Submitted };
                var approvedStatusList = new List<ProjectMilestoneStatus>()
                {
                    ProjectMilestoneStatus.Completed,
                    ProjectMilestoneStatus.Warning
                };
                var warnStatusList = new List<ProjectMilestoneStatus>()
                {
                    ProjectMilestoneStatus.Resubmitted,
                };
                var reSubmittedStatusList = new List<ProjectMilestoneStatus>()
                {
                    ProjectMilestoneStatus.Completed,
                    ProjectMilestoneStatus.Failed
                };
                // check project milestone incoming status
                // ...
                bool statusChanged = false;
                if (projectMilestone.Milestone.MilestoneOrder != 1)
                {
                    var checkValidateMilstone = CanUpdateProjectMilestone(fundingProject, projectMilestone.Milestone.MilestoneOrder);
                    if (checkValidateMilstone != null)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, checkValidateMilstone);
                    }
                }

                if (projectMilestone.Status == ProjectMilestoneStatus.Pending && pendingStatusList.Contains(request.Status))
                {
                    if (projectMilestone.Milestone.MilestoneOrder == 1)
                    {
                        await ChargeCommissionFee(projectMilestone.Id);
                    }

                    projectMilestone.Status = request.Status;
                    statusChanged = true;
                    await TransferHalfMilestone(projectMilestone.Id, 1);

                }
                else if (projectMilestone.Status == ProjectMilestoneStatus.Processing && processingStatusList.Contains(request.Status))
                {
                    projectMilestone.Status = request.Status;
                    statusChanged = true;
                    //warning
                    if (request.Status.Equals(ProjectMilestoneStatus.Warning))
                    {
                        if (request.NewEndDate == null)
                        {
                            return ResultDTO<string>.Fail("New end date is required for warning a project milestone!");
                        }
                        else
                        {
                            projectMilestone.EndDate = request.NewEndDate.Value;
                        }
                    }
                    //submitted
                    if ((request.Status.Equals(ProjectMilestoneStatus.Submitted)))
                    {
                        var check = CheckMandatoryRequirement(projectMilestone.Id);
                        if (check)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "Mandatory requirements must be completed before submitting");
                        }
                        await ChangeRequirementDone(projectMilestone.Id);
                    }

                }
                else if (projectMilestone.Status == ProjectMilestoneStatus.Submitted && approvedStatusList.Contains(request.Status))
                {
                    projectMilestone.Status = request.Status;
                    statusChanged = true;
                    if (projectMilestone.Status == ProjectMilestoneStatus.Completed)
                    {
                        TransferHalfMilestone(projectMilestone.Id, 2);
                    }
                    if (projectMilestone.Milestone.MilestoneOrder == 4)
                    {
                        await ChangeProjectSuccessful(projectMilestone.FundingProjectId);
                    }
                    projectMilestone.EndDate = DateTime.Now;
                }
                else if (projectMilestone.Status == ProjectMilestoneStatus.Warning && warnStatusList.Contains(request.Status))
                {
                    projectMilestone.Status = request.Status;
                    statusChanged = true;
                }
                else if (projectMilestone.Status == ProjectMilestoneStatus.Resubmitted && reSubmittedStatusList.Contains(request.Status))
                {
                    projectMilestone.Status = request.Status;
                    statusChanged = true;
                    if (projectMilestone.Status == ProjectMilestoneStatus.Failed)
                    {
                        await RefundBackersAsync(projectMilestone.Id);
                        await FailedProjectWhenMilestoneFailed(projectMilestone.FundingProjectId);
                    }
                    if (projectMilestone.Status == ProjectMilestoneStatus.Completed)
                    {
                        if (projectMilestone.Milestone.MilestoneOrder == 4)
                        {
                            await ChangeProjectSuccessful(projectMilestone.FundingProjectId);
                        }
                        await TransferHalfMilestone(projectMilestone.Id, 2);
                        await ChangeRequirementDone(projectMilestone.Id);
                        projectMilestone.EndDate = DateTime.Now;
                    }
                }
                if (statusChanged)
                {
                    _unitOfWork.ProjectMilestoneRepository.Update(projectMilestone);

                    await _unitOfWork.CommitAsync();
                }
                else throw new ExceptionError(
                       (int)HttpStatusCode.BadRequest,
                       $"Milestone with status {projectMilestone.Status} cannot be changed to {request.Status}.");

                return ResultDTO<string>.Success($"Update successfully to {request.Status}!");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new Exception(ex.Message);
            }
        }
        public async Task ChargeCommissionFee(Guid projectMilestoneId)
        {
            try
            {

                var projectMilestone = await _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                   .Include(pm => pm.FundingProject.Wallet)
                   .Include(pm => pm.Milestone)
                   .FirstOrDefaultAsync(pm => pm.Id == projectMilestoneId);
                var commissionFee = _unitOfWork.CommissionFeeRepository.GetQueryable()
                                .Where(c => c.CommissionType == CommissionType.FundingCommission)
                                .OrderByDescending(c => c.UpdateDate)
                                .FirstOrDefault() ?? throw new ExceptionError((int)HttpStatusCode.NotFound, "Commission fee not found");
                var balance = projectMilestone.FundingProject.Wallet.Balance;
                if (balance <= 0)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project balance cannot be 0 or negative");
                }
                else if (balance < balance * (1 - commissionFee.Rate))
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project balance is not available for charging");
                }
                projectMilestone.FundingProject.Wallet.Balance *= (1 - commissionFee.Rate);
                var systemWallet = await _unitOfWork.SystemWalletRepository.GetAsync(s => true)
                    ?? throw new ExceptionError((int)HttpStatusCode.NotFound, "System wallet not found");
                var transaction = new Transaction
                {
                    WalletId = projectMilestone.FundingProject.Wallet.Id,
                    TotalAmount = -(commissionFee.Rate * balance),
                    TransactionType = TransactionTypes.CommissionFee,
                    CreatedDate = DateTime.Now,
                    Description = "Charge Commission Fee",
                    ProjectMilestoneId = projectMilestone.Id,
                    CommissionFeeId = commissionFee.Id,
                };

                systemWallet.TotalAmount += commissionFee.Rate * balance;
                _unitOfWork.TransactionRepository.Add(transaction);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new Exception(ex.Message);
            }
        }
        public async Task RefundBackersAsync(Guid projectMilestoneId)
        {
            try
            {
                // Get the project milestone
                var projectMilestone = await _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                    .Include(pm => pm.FundingProject.Wallet)
                    .Include(pm => pm.Milestone)
                    .FirstOrDefaultAsync(pm => pm.Id == projectMilestoneId);

                if (projectMilestone == null)
                    throw new Exception("Project milestone not found.");

                // Check if the milestone status is "Failed"
                if (projectMilestone.Status != ProjectMilestoneStatus.Failed)
                    throw new Exception("Milestone is not in a failed state.");

                // Get all milestones for the funding project in order
                var milestones = await _unitOfWork.MilestoneRepository.GetQueryable()
                    .Where(m => m.ProjectMilestones.Any(pm => pm.FundingProjectId == projectMilestone.FundingProjectId))
                    .OrderBy(m => m.MilestoneOrder)
                    .ToListAsync();

                // Calculate the disbursement percentage for completed milestones 
                decimal completedDisbursementPercentage = (projectMilestone.Milestone.DisbursementPercentage * 0.5m);
                foreach (var milestone in milestones)
                {
                    var relatedProjectMilestone = _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                        .Include(pm => pm.Milestone)
                        .FirstOrDefault(pm => pm.MilestoneId == milestone.Id && pm.FundingProjectId == projectMilestone.FundingProjectId);

                    if (relatedProjectMilestone != null && relatedProjectMilestone.Status == ProjectMilestoneStatus.Completed &&
                        relatedProjectMilestone.Milestone.MilestoneOrder < projectMilestone.Milestone.MilestoneOrder)
                    {
                        completedDisbursementPercentage += milestone.DisbursementPercentage;
                    }
                }
                decimal refundablePercentage = 1m - completedDisbursementPercentage;

                // Calculate the refundable amount
                var commissionFee = _unitOfWork.CommissionFeeRepository.GetQueryable()
                                .Where(c => c.CommissionType == CommissionType.FundingCommission)
                                .OrderByDescending(c => c.UpdateDate)
                                .FirstOrDefault();
                decimal totalFunds = projectMilestone.FundingProject.Balance;
                decimal refundableAmount = projectMilestone.FundingProject.Wallet.Balance;

                // Get all backers for the funding project
                var packageBackers = await _unitOfWork.PackageBackerRepository.GetQueryable()
                    .Include(pb => pb.User)
                    .Where(pb => pb.Package.ProjectId == projectMilestone.FundingProjectId)
                    .ToListAsync();

                // Calculate total contribution by all backers
                decimal totalContribution = packageBackers.Sum(pb => pb.DonateAmount);

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
                        ProjectMilestoneId = projectMilestone.Id
                    };
                    await _unitOfWork.TransactionRepository.AddAsync(transaction);
                }
                projectMilestone.FundingProject.Wallet.Balance -= refundableAmount;

                // Save changes to the database
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

        public async Task FailedProjectWhenMilestoneFailed(Guid projectId)
        {
            try
            {
                var project = _unitOfWork.FundingProjectRepository.GetQueryable()
                    .Include(p => p.ProjectMilestones)
                    .FirstOrDefault(p => p.Id == projectId);

                if (project == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project not found");
                }
                project.Status = ProjectStatus.Failed;
                foreach (ProjectMilestone pm in project.ProjectMilestones)
                {
                    pm.Status = ProjectMilestoneStatus.Failed;
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new Exception(ex.Message);
            }
        }

        public bool CheckMandatoryRequirement(Guid projectMilestoneId)
        {
            try
            {
                var projectMilestone = _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                    .Include(pm => pm.ProjectMilestoneRequirements)
                    .ThenInclude(pmr => pmr.Requirement)
                    .FirstOrDefault(pm => pm.Id == projectMilestoneId);
                var quillEmptyPlaceholders = new HashSet<string> { "<p><br></p>", "<p></p>" };
                foreach (ProjectMilestoneRequirement req in projectMilestone.ProjectMilestoneRequirements)
                {
                    if ((string.IsNullOrWhiteSpace(req.Content) || quillEmptyPlaceholders.Contains(req.Content.Trim())) && req.Requirement.Status == FixedRequirementStatus.Mandatory)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new Exception(ex.Message);
            }
        }

        public async Task TransferHalfMilestone(Guid projectMilestoneId, int status)
        {
            try
            {
                var projectMilestone = _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                    .Include(pm => pm.Milestone)
                    .Include(pm => pm.FundingProject.Wallet)
                    .FirstOrDefault(pm => pm.Id == projectMilestoneId);
                var rate = projectMilestone.Milestone.DisbursementPercentage;
                var commissionFee = _unitOfWork.CommissionFeeRepository.GetQueryable()
                               .Where(c => c.CommissionType == CommissionType.FundingCommission)
                               .OrderByDescending(c => c.UpdateDate)
                               .FirstOrDefault();
                var refundableAmount = (1 - commissionFee.Rate) * projectMilestone.FundingProject.Balance;
                var transferMoney = (rate * 0.5m) * refundableAmount;
                if (status == 1 || status == 2)
                {
                    if (projectMilestone.FundingProject.Wallet.Balance < transferMoney)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Wallet is not enough for transferring money");
                    }
                    var transaction = new Transaction
                    {
                        WalletId = projectMilestone.FundingProject.Wallet.Id,
                        TotalAmount = -transferMoney,
                        TransactionType = status == 1 ? TransactionTypes.MilestoneFirstHalf : TransactionTypes.MilestoneSecondHalf,
                        CreatedDate = DateTime.Now,
                        Description = "Transfer money to milestone disbursement",
                        ProjectMilestoneId = projectMilestone.Id
                    };
                    projectMilestone.FundingProject.Wallet.Balance -= transferMoney;
                    _unitOfWork.TransactionRepository.Add(transaction);
                    _unitOfWork.Commit();
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Not support this mode");
                }


            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new Exception(ex.Message);
            }
        }

        public async Task ChangeProjectSuccessful(Guid projectId)
        {
            try
            {
                var project = _unitOfWork.FundingProjectRepository.GetQueryable()
                   .Include(p => p.ProjectMilestones)
                   .FirstOrDefault(p => p.Id == projectId);

                if (project == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project not found");
                }
                project.Status = ProjectStatus.Successful;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResultDTO<PaginatedResponse<ProjectMilestoneResponse>>> GetProjectMilestones(
            ListRequest request,
            ProjectMilestoneStatus? status,
            Guid? fundingProjectId,
            Guid? milestoneId)
        {
            try
            {
                // Initialize the filter with a default condition that always evaluates to true.
                Expression<Func<ProjectMilestone, bool>> filter = u => true;

                // Apply status filter.
                if (status != null)
                {
                    filter = u => u.Status == status;
                }

                // Apply FundingProjectId and milestoneId filters.
                if (fundingProjectId != null && milestoneId != null)
                {
                    // Both parameters are provided, so combine them with &&.
                    filter = u => u.FundingProjectId == fundingProjectId && u.MilestoneId == milestoneId;
                }
                else if (fundingProjectId != null)
                {
                    // Only FundingProjectId is provided.
                    filter = u => u.FundingProjectId == fundingProjectId;
                }
                else if (milestoneId != null)
                {
                    // Only milestoneId is provided.
                    filter = u => u.MilestoneId == milestoneId;
                }

                // Apply date filters.
                if (request.From is DateTime fromDate)
                {
                    filter = u => u.CreatedDate >= fromDate;
                }

                if (request.To is DateTime toDate)
                {
                    filter = u => u.EndDate <= toDate;
                }

                // Define the orderBy expression.
                Expression<Func<ProjectMilestone, object>> orderBy = u => u.CreatedDate;
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "enddate":
                            orderBy = u => u.EndDate;
                            break;
                        case "status":
                            orderBy = u => u.Status;
                            break;
                        default:
                            break;
                    }
                }

                // Retrieve the paginated list of milestones.
                var list = await _unitOfWork.ProjectMilestoneRepository.GetAllAsync(
                    filter: filter,
                    orderBy: orderBy,
                    isAscending: request.IsAscending ?? true,
                    pageIndex: request.PageIndex ?? 1,
                    pageSize: request.PageSize ?? 10,
                    includeProperties: "Milestone,FundingProject,FundingProject.SourceFiles,FundingProject.User," +
                    "FundingProject.Packages,FundingProject.Packages.PackageUsers,FundingProject.Wallet,FundingProject.Wallet.BankAccount" +
                    ",ProjectMilestoneRequirements.RequirementFiles,ProjectMilestoneRequirements.Requirement"
                );

                var totalItems = _unitOfWork.ProjectMilestoneRepository.GetAll(filter).Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / (request.PageSize ?? 10));

                // Map to the response DTO.
                var responseItems = _mapper.Map<IEnumerable<ProjectMilestoneResponse>>(list);

                var response = new PaginatedResponse<ProjectMilestoneResponse>
                {
                    PageSize = request.PageSize ?? 10,
                    PageIndex = request.PageIndex ?? 1,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = responseItems
                };

                foreach (var item in responseItems)
                {
                    var fundingProjectIdForMilestone = item.FundingProject.Id;

                    // Retrieve the latest milestone for the funding project.
                    var latestMilestone = await _unitOfWork.ProjectMilestoneRepository.GetAllAsync(
                        filter: pm => pm.FundingProjectId == fundingProjectIdForMilestone && pm.Milestone != null,
                        includeProperties: "Milestone"
                    );

                    var orderedLatestMilestone = latestMilestone
                        .OrderByDescending(pm => pm.Milestone.MilestoneOrder)
                        .Select(pm => pm.Milestone)
                        .FirstOrDefault();

                    var milestoneRes = _mapper.Map<MilestoneResponse>(orderedLatestMilestone);

                    item.LatestMilestone = milestoneRes;
                    item.ProjectStatus = item.FundingProject.Status;
                }

                return ResultDTO<PaginatedResponse<ProjectMilestoneResponse>>.Success(response);

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

        public async Task ChangeRequirementDone(Guid projectMilestoneId)
        {
            try
            {
                var projectMilestone = _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                .Include(pm => pm.ProjectMilestoneRequirements).FirstOrDefault(pm => pm.Id == projectMilestoneId);
                if (projectMilestone == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Project milestone is not found");
                }
                foreach (ProjectMilestoneRequirement req in projectMilestone.ProjectMilestoneRequirements)
                {
                    req.RequirementStatus = RequirementStatus.Done;
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new Exception(ex.Message);
            }


        }

        public async Task<ResultDTO<List<ProjectMilestoneResponse>>> GetProjectMilestonesByProjectAndMilestone(
            Guid? fundingProjectId,
            Guid? milestoneId)
        {
            try
            {
                var list = await _unitOfWork.ProjectMilestoneRepository.GetAllAsync(
                filter: pm => (!fundingProjectId.HasValue || pm.FundingProjectId == fundingProjectId.Value) &&
           (!milestoneId.HasValue || pm.MilestoneId == milestoneId.Value),
                 includeProperties: "Milestone.Requirements,ProjectMilestoneRequirements,ProjectMilestoneRequirements.RequirementFiles,ProjectMilestoneRequirements.Requirement"
                );
                var response = _mapper.Map<List<ProjectMilestoneResponse>>(list);
                return ResultDTO<List<ProjectMilestoneResponse>>.Success(response);

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

        public async Task<ResultDTO<object>> WithdrawMilestoneProcessing(Guid id)
        {
            try
            {
                var projectMilestone = _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                    .Include(pm => pm.FundingProject.Wallet)
                    .Include(pm => pm.Milestone)
                    .FirstOrDefault(pm => pm.Id == id);
                if (projectMilestone == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "This milestone not found");
                }
                var walletId = projectMilestone.FundingProject.Wallet.Id;
                var withdrawTrans = _unitOfWork.TransactionRepository.GetQueryable()
                    .FirstOrDefault(t => t.WalletId == walletId && t.TransactionType == TransactionTypes.WithdrawFundingMilestone);
                if (withdrawTrans != null)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "This project has been withdrawed in funding process");
                }
               
                if (projectMilestone.Milestone.MilestoneType != MilestoneType.Funding)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Withdraw must be requested for funding process");
                }
                if (projectMilestone.FundingProject.Status != ProjectStatus.Processing)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Withdraw must be requested while project is processing");
                }
                if (projectMilestone.FundingProject.Balance < (projectMilestone.Milestone.DisbursementPercentage * projectMilestone.FundingProject.Target))
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Donation progress has not reached the requirement");
                }
                var transferAmount = (projectMilestone.FundingProject.Target * (projectMilestone.Milestone.DisbursementPercentage / 2));
                var withdrawRequest = new WithdrawRequest
                {
                    Amount = transferAmount,
                    WalletId = projectMilestone.FundingProject.Wallet.Id,
                    Status = WithdrawRequestStatus.Pending,
                    CreatedDate = DateTime.Now,
                    ExpiredDate = DateTime.Now.AddDays(7)
                };
                var transaction = new Transaction
                {
                    TotalAmount = transferAmount,
                    WalletId = projectMilestone.FundingProject.Wallet.Id,
                    TransactionType = TransactionTypes.WithdrawFundingMilestone,
                    CreatedDate = DateTime.Now,
                    Description = "Withdraw money in funding process of" + projectMilestone.FundingProject.Name
                };
                _unitOfWork.WithdrawRequestRepository.Add(withdrawRequest);
                _unitOfWork.TransactionRepository.Add(transaction);
                projectMilestone.FundingProject.Wallet.Balance -= transferAmount;
                _unitOfWork.Commit();

                return ResultDTO<object>.Success(projectMilestone);
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
