using AutoMapper;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ReportDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class ReportService : IReportService
    {
        private readonly IUserService _userService;
        private readonly IBackgroundProcessService _backgroundProcessService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAzureService _azureService;

        public ReportService(IUserService userService,IBackgroundProcessService backgroundProcessService, IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IAzureService azureService)
        {
            _userService = userService;
            _backgroundProcessService = backgroundProcessService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _azureService = azureService;
        }

        public async Task<ResultDTO<ViolentReport>> CreateReportRequest(ReportRequest request)
        {
            var user = await _userService.GetUserInfo();
            User exitUser = _mapper.Map<User>(user._data);

            if (exitUser == null)
            {
                return ResultDTO<ViolentReport>.Fail("user must be authenticate");
            }
            if (request is null)
            {
                return ResultDTO<ViolentReport>.Fail("field can not be null");
            }
            switch (request.Type)
            {
                case ReportType.User:
                    var exitedUser = await _unitOfWork.UserRepository.GetByIdAsync(request.ViolatorId);
                    if (exitedUser is null)
                    {
                        return ResultDTO<ViolentReport>.Fail("user can not found");
                    }
                    break;
                case ReportType.MarketplaceProject:
                    var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetByIdAsync(request.ViolatorId);
                    if (marketplaceProject is null)
                    {
                        return ResultDTO<ViolentReport>.Fail("can not found project");
                    }
                    break;
                case ReportType.FundingProject:
                    var exitedProject = await _unitOfWork.FundingProjectRepository.GetByIdAsync(request.ViolatorId);
                    if (exitedProject is null)
                    {
                        return ResultDTO<ViolentReport>.Fail("can not found project");
                    }
                    break;
                default:
                    return ResultDTO<ViolentReport>.Fail("Invalid report type.");
            }
            try
            {
                // Upload files and get their URLs
                
                List<string> fileUrls = new List<string>();
                if (request.FileUrls is not null)
                {
                    foreach (var file in request.FileUrls) // `Files` would be of type `IFormFile[]`
                    {
                        // Upload each file and get its URL (pseudo-code)
                        string fileUrl = await _azureService.UploadUrlSingleFiles(file);
                        fileUrls.Add(fileUrl);
                    }
                }
                
                ViolentReport report = new ViolentReport
                {
                    Id = Guid.NewGuid(),
                    FileUrls = fileUrls,
                    ViolatorId = request.ViolatorId,
                    ReporterId = exitUser.Id,
                    Content = request.Content,
                    IsHandle = false,
                    Date = DateTime.Now,
                    Type = request.Type,
                    FaultCauses = request.FaultCauses,
                };
                _unitOfWork.ReportRepository.Create(report);
                return ResultDTO<ViolentReport>.Success(report, "Successfull to create report");

            }
            catch (Exception ex)
            {
                return ResultDTO<ViolentReport>.Fail("something wrong!");
            }
        }

        public async Task<ResultDTO<PaginatedResponse<ViolentReport>>> GetAllReport(ListRequest request)
        {
            try
            {
                Expression<Func<ViolentReport, bool>> filter = null;
                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    filter = c => c.Type.Equals(request.SearchValue);
                }
                var list = _unitOfWork.ReportRepository.GetAllPaged(request,filter);
                return ResultDTO<PaginatedResponse<ViolentReport>>.Success(list, "Successfull querry");
            }
            catch (Exception ex)
            {
                return ResultDTO<PaginatedResponse<ViolentReport>>.Fail("something wrong!");
            }
        }

        public async Task<ResultDTO<string>> SendReportedEmail(EmailReportRequest request)
        {
            var exitedReport = _unitOfWork.ReportRepository.Get(x => x.Id == request.ReportId);
            if (exitedReport is null)
            {
                return ResultDTO<string>.Fail("can not find any reports");
            }
            try
            {
                if (exitedReport.Type == ReportType.User)
                {
                    var user = await _unitOfWork.UserRepository.GetByIdAsync(exitedReport.ViolatorId);
                    await _emailService.SendUserReportAsync(
                        toEmail: user.Email, 
                        userName: user.FullName, 
                        reportedDate: exitedReport.Date,
                        reason:exitedReport.FaultCauses,
                        content: request.Content);
                    return ResultDTO<string>.Success("Successfull send email");
                }
                else
                {
                    if (exitedReport.Type == ReportType.FundingProject)
                    {
                        var fundingProject = await _unitOfWork.FundingProjectRepository.GetByIdAsync(exitedReport.ViolatorId);
                        var owner = await _unitOfWork.UserRepository.GetByIdAsync(fundingProject.UserId);
                        await _emailService.SendReportAsync(toEmail: owner.Email,
                            userName: owner.FullName,
                            reportedDate: exitedReport.Date,
                            reason: exitedReport.FaultCauses,
                            projectName: fundingProject.Name,
                            content: request.Content);
                        return ResultDTO<string>.Success("Successfull send email");

                    }
                    else
                    {
                        var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetByIdAsync(exitedReport.ViolatorId);
                        var fundingProject = await _unitOfWork.FundingProjectRepository.GetByIdAsync(marketplaceProject.FundingProjectId);
                        var owner = await _unitOfWork.UserRepository.GetByIdAsync(fundingProject.UserId);
                        await _emailService.SendReportAsync(toEmail: owner.Email,
                            userName: owner.FullName,
                            reportedDate: exitedReport.Date,
                            reason: exitedReport.FaultCauses,
                            projectName: fundingProject.Name,
                            content: request.Content);
                        return ResultDTO<string>.Success("Successfull send email");
                    }
                }

            }
            catch (Exception ex)
            {
                return ResultDTO<string>.Fail(ex.Message);
            }
        }

        public async Task<ResultDTO<ViolentReport>> UpdateHandleReport(Guid id)
        {
            var user = await _userService.GetUserInfo();
            User exitUser = _mapper.Map<User>(user._data);
            if (exitUser == null)
                return ResultDTO<ViolentReport>.Fail("user not authenticate");
            var exitedReport = _unitOfWork.ReportRepository.Get(x => x.Id == id);
            if (exitedReport == null)
                return ResultDTO<ViolentReport>.Fail("reportId null");

            
            switch (exitedReport.Type)
            {
                case ReportType.User:
                    var exitedUser = await _unitOfWork.UserRepository.GetByIdAsync(exitedReport.ViolatorId);
                    exitedUser.UserStatus = UserStatus.Inactive; 
                    await _unitOfWork.CommitAsync();
                    if (exitedUser is null)
                    {
                        return ResultDTO<ViolentReport>.Fail("user can not found");
                    }
                   
                    break;
                case ReportType.MarketplaceProject:
                    var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetByIdAsync(exitedReport.ViolatorId);
                    marketplaceProject.Status = ProjectStatus.Reported;
                    await _unitOfWork.CommitAsync();
                    if (marketplaceProject is null)
                    {
                        return ResultDTO<ViolentReport>.Fail("can not found project");
                    }
                    
                    break;
                case ReportType.FundingProject:
                    var exitedProject = await _unitOfWork.FundingProjectRepository.GetByIdAsync(exitedReport.ViolatorId);
                    exitedProject.Status = ProjectStatus.Reported;
                    await _backgroundProcessService.RefundFundingBackers(exitedProject.Id);
                    await _unitOfWork.CommitAsync();
                    if (exitedProject is null)
                    {
                        return ResultDTO<ViolentReport>.Fail("can not found project");
                    }
                 
                    break;
                default:
                    return ResultDTO<ViolentReport>.Fail("Invalid report type.");
            }          
            try
            {
                var reporter = await _unitOfWork.UserRepository.GetByIdAsync(exitedReport.ReporterId);
                var update = Builders<ViolentReport>.Update.Set(x => x.IsHandle, true);
                _unitOfWork.ReportRepository.Update(x => x.Id == id, update);
                var response = _unitOfWork.ReportRepository.Get(x => x.Id == id);
                return ResultDTO<ViolentReport>.Success(response, "Successfull updated");
            }
            catch (Exception ex)
            {
                return ResultDTO<ViolentReport>.Fail("something wrong!");
            }
        }


    }
}
