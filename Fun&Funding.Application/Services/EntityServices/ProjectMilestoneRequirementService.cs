using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ProjectMilestoneDTO;
using Fun_Funding.Application.ViewModel.ProjectMilestoneRequirementDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class ProjectMilestoneRequirementService : IProjectMilestoneRequirementService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private IAzureService _azureService;
        private IProjectMilestoneService _projectMilestoneService;
        private readonly IMilestoneService _milestoneService;
        private DateTime present = DateTime.Now;
        public ProjectMilestoneRequirementService(IUnitOfWork unitOfWork, IMapper mapper, IAzureService azureService, IProjectMilestoneService projectMilestoneService, IMilestoneService milestoneService, ISystemWalletService systemWalletService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _azureService = azureService;
            _projectMilestoneService = projectMilestoneService;
            _milestoneService = milestoneService;

        }
        public async Task<ResultDTO<ProjectMilestoneResponse>> CreateMilestoneRequirements(List<ProjectMilestoneRequirementRequest> request, string? issueLog )
        {
            try
            {
                var projectMilestone = _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                    .Include( x => x.Milestone )
                    .Include(x =>  x.FundingProject)
                    .FirstOrDefault(pm => pm.FundingProjectId == request[0].FundingProjectId && pm.MilestoneId == request[0].MilestoneId);
                if (issueLog != null) { 
                    projectMilestone.IssueLog = issueLog;
                }
                if (projectMilestone == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Milestone for this project not found");
                }
                else
                {
                    FundingProject project = _unitOfWork.FundingProjectRepository
                    .GetQueryable().Include(p => p.ProjectMilestones)
                    .ThenInclude(pm => pm.Milestone).FirstOrDefault(p => p.Id == projectMilestone.FundingProjectId);
                    if (projectMilestone.Milestone.MilestoneType == MilestoneType.Disbursement)
                    {
                        var checkValidateMilstone = _projectMilestoneService.CanCreateProjectMilestone(project, projectMilestone.Milestone.MilestoneOrder, projectMilestone.CreatedDate);
                        if (checkValidateMilstone != null)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, checkValidateMilstone);
                        }
                    }

                    if (projectMilestone.Milestone.MilestoneType == MilestoneType.Funding)
                    {
                        if (projectMilestone.FundingProject.Status != ProjectStatus.Approved && projectMilestone.FundingProject.Status != ProjectStatus.Processing)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "Project is not in valid status");
                        }
                    }
                    
                    if (projectMilestone.Status != ProjectMilestoneStatus.Processing)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest, "Milestone for this project is not approved yet");
                    }
                }

                foreach (var requestItem in request)
                {
                    requestItem.Content = string.IsNullOrWhiteSpace(requestItem.Content) ? " " : requestItem.Content;
                    ProjectMilestoneRequirement req = new ProjectMilestoneRequirement
                    {
                        ProjectMilestoneId = projectMilestone.Id,
                        Content = requestItem.Content,
                        RequirementId = requestItem.RequirementId,
                        RequirementStatus = requestItem.RequirementStatus,
                        CreatedDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    List<ProjectRequirementFile> files = new List<ProjectRequirementFile>();
                    if (requestItem.RequirementFiles?.Count > 0)
                    {
                        foreach (var file in requestItem.RequirementFiles)
                        {
                            if (file.URL.Length > 0)
                            {
                                var url = _azureService.UploadUrlSingleFiles(file.URL);
                                ProjectRequirementFile requirementFile = new ProjectRequirementFile
                                {
                                    Name = file.Name,
                                    URL = url.Result,
                                    CreatedDate = DateTime.Now,
                                    File = file.Filetype
                                };
                                files.Add(requirementFile);
                            }
                        }
                    }
                    req.RequirementFiles = files;
                    _unitOfWork.ProjectMilestoneRequirementRepository.Add(req);

                }
                await _unitOfWork.CommitAsync();
                var response = await _projectMilestoneService.GetProjectMilestoneRequest(projectMilestone.Id);
                return response;
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

        public async Task<ResultDTO<string>> UpdateMilestoneRequirements(List<ProjectMilestoneRequirementUpdateRequest> request, string? issueLog)
        {
            try
            {

                ProjectMilestoneRequirement sampleReq = _unitOfWork.ProjectMilestoneRequirementRepository
                    .GetQueryable().Include(pmr => pmr.RequirementFiles).FirstOrDefault(pmr => pmr.Id == request[0].Id);
                var projectMilestone = _unitOfWork.ProjectMilestoneRepository.GetQueryable()
                    .Include(x => x.Milestone)
                    .Include(x => x.FundingProject)
                    .FirstOrDefault(pm => pm.Id == sampleReq.ProjectMilestoneId);
                if (projectMilestone == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Milestone for this project not found");
                }
                else
                {
                    
                    FundingProject project = _unitOfWork.FundingProjectRepository
                    .GetQueryable().Include(p => p.ProjectMilestones)
                    .ThenInclude(pm => pm.Milestone).FirstOrDefault(p => p.Id == projectMilestone.FundingProjectId);
                    if (project.Status == ProjectStatus.Failed)
                    {
                        throw new ExceptionError((int)HttpStatusCode.BadRequest,"Project is failed");
                    }
                    if (projectMilestone.Milestone.MilestoneType == MilestoneType.Disbursement)
                    {
                        var checkValidateMilstone = _projectMilestoneService.CanCreateProjectMilestone(project, projectMilestone.Milestone.MilestoneOrder, projectMilestone.CreatedDate);
                        if (checkValidateMilstone != null)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, checkValidateMilstone);
                        }
                        if (projectMilestone.Status != ProjectMilestoneStatus.Processing && projectMilestone.Status != ProjectMilestoneStatus.Warning)
                        {
                            throw new ExceptionError((int)HttpStatusCode.BadRequest, "Milestone for this project is not approved yet");
                        }
                    }
                    
                    
                }
                if (issueLog != null)
                {
                    projectMilestone.IssueLog = issueLog;
                }
               
                //Projec
                foreach (var requestItem in request)
                {
                    ProjectMilestoneRequirement req = _unitOfWork.ProjectMilestoneRequirementRepository
                        .GetQueryable().Include(pmr => pmr.RequirementFiles).FirstOrDefault(pmr => pmr.Id == requestItem.Id);
                    
                    req.Content = requestItem.Content;
                    req.CreatedDate = DateTime.Now;
                    req.UpdateDate = DateTime.Now;
                    req.RequirementStatus = requestItem.RequirementStatus;
                    if (requestItem.RequirementFiles != null)
                    {
                        foreach (var file in requestItem.RequirementFiles)
                        {
                            ProjectRequirementFile reqFile = _unitOfWork.ProjectRequirementFileRepository.GetById(file.Id);
                            reqFile.URL = file.URL;
                            reqFile.Name = file.Name;
                            reqFile.IsDeleted = file.IsDeleted;
                        }
                    }
                    if (requestItem.AddedFiles?.Count > 0)
                    {
                        foreach (var file in requestItem.AddedFiles)
                        {
                            if (file.URL.Length > 0)
                            {
                                var url = _azureService.UploadUrlSingleFiles(file.URL);
                                ProjectRequirementFile requirementFile = new ProjectRequirementFile
                                {
                                    Name = file.Name,
                                    URL = url.Result,
                                    CreatedDate = DateTime.Now,
                                    File = file.Filetype
                                };
                                req.RequirementFiles.Add(requirementFile);
                            }
                        }
                    }
                    _unitOfWork.ProjectMilestoneRequirementRepository.Update(req);
                }
                if ((projectMilestone.EndDate.Date - present.Date).TotalDays <= 0)
                {
                    if (projectMilestone.Status == ProjectMilestoneStatus.Processing)
                    {
                        projectMilestone.Status = ProjectMilestoneStatus.Submitted;

                    }
                    else if (projectMilestone.Status == ProjectMilestoneStatus.Warning)
                    {
                        projectMilestone.Status = ProjectMilestoneStatus.Resubmitted;
                    }
                    _unitOfWork.Commit();
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "This milestone has already been expired");
                }
                _unitOfWork.Commit();
                return ResultDTO<string>.Success("Ok");
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }

                throw new Exception($"Could not update {ex.Message}");
            }
        }




    }
}
