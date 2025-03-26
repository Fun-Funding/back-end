using AutoMapper;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ProjectMilestoneBackerDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class ProjectMilestoneBackerService : IProjectMilestoneBackerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public ProjectMilestoneBackerService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<ResultDTO<bool>> CheckIfQualifiedForReview(Guid projectMilestoneId, Guid userId)
        {
            try
            {
                var fundingProject = await _unitOfWork.FundingProjectRepository
                    .GetAsync(p => p.ProjectMilestones.Any(pm => pm.Id == projectMilestoneId));
                if (fundingProject == null)
                    return ResultDTO<bool>.Fail("Funding project not found!");

                var isBacker = await _unitOfWork.PackageBackerRepository.GetQueryable()
                    .AnyAsync(pb => pb.Package.ProjectId == fundingProject.Id && pb.UserId == userId);
                if (!isBacker)
                    return ResultDTO<bool>.Success(false, "You must be a backer to review this milestone!");

                var projectMilestone = await _unitOfWork.ProjectMilestoneRepository
                    .GetAsync(pm => pm.Id == projectMilestoneId);
                if (projectMilestone == null)
                    return ResultDTO<bool>.Fail("Project milestone not found!");
                if (projectMilestone.Status != ProjectMilestoneStatus.Submitted &&
                    projectMilestone.Status != ProjectMilestoneStatus.Resubmitted)
                    return ResultDTO<bool>.Success(false, "This project milestone is currently not accepting reviews!");

                var alreadyReviewed = await _unitOfWork.ProjectMilestoneBackerRepository.GetQueryable()
                    .AnyAsync(pmb => pmb.ProjectMilestoneId == projectMilestoneId && pmb.BackerId == userId);
                if (alreadyReviewed)
                    return ResultDTO<bool>.Success(false, "You have already reviewed this milestone!");

                return ResultDTO<bool>.Success(true, "You are qualified to review this milestone.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while checking review qualifications: {ex.Message}");
            }
        }

        public async Task<ResultDTO<ProjectMilestoneBackerResponse>> CreateNewProjectMilestoneBackerReview(ProjectMilestoneBackerRequest request)
        {
            try
            {
                var backer = await _unitOfWork.UserRepository.GetAsync(u => u.Id == request.BackerId);
                if (backer == null) return ResultDTO<ProjectMilestoneBackerResponse>.Fail("Backer not found!");

                var projectMilestone = await _unitOfWork.ProjectMilestoneRepository.GetAsync(m => m.Id == request.ProjectMilestoneId);
                if (projectMilestone == null) return ResultDTO<ProjectMilestoneBackerResponse>.Fail("Project milestone not found!");

                // check project milestone status
                if (projectMilestone.Status != Domain.Enum.ProjectMilestoneStatus.Submitted && projectMilestone.Status != Domain.Enum.ProjectMilestoneStatus.Resubmitted)
                    return ResultDTO<ProjectMilestoneBackerResponse>.Fail("This project milestone is currently not accepting reviews!");

                // check if backer donate
                var fundingProject = await _unitOfWork.FundingProjectRepository.GetAsync(p => p.ProjectMilestones.Any(p => p.Id == request.ProjectMilestoneId));
                if (fundingProject == null) return ResultDTO<ProjectMilestoneBackerResponse>.Fail("Funding project not found!");

                var isBacker = await _unitOfWork.PackageBackerRepository.GetQueryable()
                    .AnyAsync(pb => pb.Package.ProjectId == fundingProject.Id && pb.UserId == request.BackerId);


                // check if backer already review this projectmilestone
                var alreadyReview = await _unitOfWork.ProjectMilestoneBackerRepository.GetQueryable()
                    .AnyAsync(pmb => pmb.BackerId == request.BackerId && pmb.ProjectMilestoneId == request.ProjectMilestoneId);
                if (alreadyReview) return ResultDTO<ProjectMilestoneBackerResponse>.Fail("Backer already review this project milestone!");

                if (isBacker)
                {
                    var newReview = new ProjectMilestoneBacker
                    {
                        Star = request.Star,
                        Comment = request.Comment,
                        BackerId = request.BackerId,
                        Backer = backer,
                        ProjectMilestoneId = request.ProjectMilestoneId,
                        ProjectMilestone = projectMilestone,
                        CreatedDate = DateTime.Now,
                    };

                    _unitOfWork.ProjectMilestoneBackerRepository.Add(newReview);

                    await _unitOfWork.CommitAsync();

                    // NOTIFICATION
                    // 1. get recipientsIds
                    List<Guid> recipientsId = new List<Guid>();
                    recipientsId.Add(fundingProject.UserId); // project owner
                    // 2. initiate new Notification object
                    var notification = new Notification
                    {
                        Id = new Guid(),
                        Date = DateTime.Now,
                        Message = $"post a milestone review of project <b>{fundingProject.Name}</b>",
                        NotificationType = NotificationType.ProjectMilestoneStatus,
                        Actor = new { backer.Id, backer.UserName, backer.File?.URL },
                        ObjectId = fundingProject.Id,
                    };
                    // 3. send noti
                    await _notificationService.SendNotification(notification, recipientsId);

                    var response = _mapper.Map<ProjectMilestoneBackerResponse>(newReview);
                    return ResultDTO<ProjectMilestoneBackerResponse>.Success(response, "Add review successfully!");
                }

                return ResultDTO<ProjectMilestoneBackerResponse>.Fail("You must be a backer to review this milestone!");


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        public async Task<ResultDTO<List<ProjectMilestoneBackerResponse>>> GetAllMilestoneReview(Guid projectMilestoneId)
        {
            try
            {
                var reviewList = await _unitOfWork.ProjectMilestoneBackerRepository
                    .GetQueryable()
                    //.Include(pmb => pmb.ProjectMilestone)
                    .Include(pmb => pmb.Backer)
                    .Where(pmb => pmb.ProjectMilestoneId == projectMilestoneId)
                    .ToListAsync();

                var responseList = new List<ProjectMilestoneBackerResponse>();

                foreach (var item in reviewList)
                {
                    responseList.Add(_mapper.Map<ProjectMilestoneBackerResponse>(item));
                }

                return ResultDTO<List<ProjectMilestoneBackerResponse>>.Success(responseList);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
