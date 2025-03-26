using AutoMapper;
using Azure.Core;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.RequirementDTO;
using Fun_Funding.Domain.Entity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class RequirementService : IRequirementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMilestoneService _milestoneService;
        private readonly IUserService _userService;

        public RequirementService(IUnitOfWork unitOfWork, IMapper mapper, IMilestoneService milestoneService, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _milestoneService = milestoneService;
            _userService = userService;
        }

        public async Task<ResultDTO<RequirementResponse>> CreateNewRequirement(RequirementRequest request)
        {
            var user = await _userService.GetUserInfo();
            if (!user._isSuccess)
                return ResultDTO<RequirementResponse>.Fail("User not found.");

            var exitUser = _mapper.Map<User>(user._data);

            if (request is null)
                return ResultDTO<RequirementResponse>.Fail("Request is null.");

            var exitedMilestone = await _unitOfWork.MilestoneRepository.GetAsync(x => x.Id == request.MilestoneId);
            if (exitedMilestone is null)
                return ResultDTO<RequirementResponse>.Fail("Milestone not found.");

            if (exitedMilestone.IsDeleted)
                return ResultDTO<RequirementResponse>.Fail("Milestone is deleted; cannot create requirement.");

            // Get all requirements for the milestone
            var exitedRequirements = await _unitOfWork.RequirementRepository.GetAllAsync(x => x.MilestoneId == request.MilestoneId);

            // Determine the next order number for the new requirement
            int nextOrder = 1;
            if (exitedRequirements.Any())
            {
                nextOrder = exitedRequirements.OrderByDescending(x => x.Order).First().Order + 1;
            }

            try
            {
                // Create the new requirement
                Requirement requirement = new Requirement
                {
                    MilestoneId = request.MilestoneId,
                    CreatedDate = DateTime.UtcNow,
                    Description = request.Description,
                    Id = Guid.NewGuid(),
                    IsDeleted = false,
                    Milestone = exitedMilestone,
                    Version = 1,
                    Title = request.Title,
                    Order = nextOrder // Use the calculated next order
                };

                await _unitOfWork.RequirementRepository.AddAsync(requirement);
                await _unitOfWork.CommitAsync();

                RequirementResponse response = _mapper.Map<RequirementResponse>(requirement);
                return ResultDTO<RequirementResponse>.Success(response, "Successfully created.");
            }
            catch (Exception ex)
            {
                // Optionally log the exception ex
                return ResultDTO<RequirementResponse>.Fail("An error occurred while creating the requirement.");
            }
        }


        public async Task<ResultDTO<RequirementResponse>> GetRequirementById(Guid id)
        {
            try
            {
                var req = await _unitOfWork.RequirementRepository.GetByIdAsync(id);
                RequirementResponse response = _mapper.Map<RequirementResponse>(req);
                return ResultDTO<RequirementResponse>.Success(response, "successfull create");
            }
            catch (Exception ex)
            {
                return ResultDTO<RequirementResponse>.Fail("something wrongs");
            }
        }

        public async Task<ResultDTO<RequirementResponse>> UpdateRequirement(UpdateRequirement request)
        {
            var user = await _userService.GetUserInfo();
            if (!user._isSuccess)
                return ResultDTO<RequirementResponse>.Fail("user null");
            User exitUser = _mapper.Map<User>(user._data);
            var requirement = _unitOfWork.RequirementRepository.GetQueryable()
                .Where(x => x.Id == request.RequirementId)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();
            if (requirement is null)
                return ResultDTO<RequirementResponse>.Fail("requirement is null");
            try
            {
                Requirement newRequirement = new Requirement
                {
                    Id = Guid.NewGuid(),
                    Version = requirement.Version + 1,
                    Order = requirement.Order,
                    Title = request.Title ?? requirement.Title,
                    CreatedDate = DateTime.Now,
                    IsDeleted = false,
                    Description = request.Description ?? requirement.Description,
                    Milestone = requirement.Milestone,
                    MilestoneId = requirement.MilestoneId,
                };
                requirement.IsDeleted = true;
                requirement.DeletedAt = DateTime.Now;
                _unitOfWork.RequirementRepository.Update(requirement);
                await _unitOfWork.RequirementRepository.AddAsync(newRequirement);
                await _unitOfWork.CommitAsync();

                RequirementResponse response = _mapper.Map<RequirementResponse>(newRequirement);
                return ResultDTO<RequirementResponse>.Success(response, "successfull update");
            }
            catch (Exception ex)
            {
                return ResultDTO<RequirementResponse>.Fail("something wrongs");
            }
        }
    }
}
