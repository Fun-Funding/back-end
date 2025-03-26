using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.FundingProjectDTO;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Application.IService
{
    public interface IFundingProjectService
    {
        Task<ResultDTO<FundingProjectResponse>> GetProjectById(Guid id);
        Task<ResultDTO<FundingProjectResponse>> GetProjectByIdAndOwner(Guid id);

        Task<ResultDTO<FundingProjectResponse>> CreateFundingProject(FundingProjectAddRequest req);
        Task<ResultDTO<FundingProjectResponse>> UpdateFundingProject(FundingProjectUpdateRequest req);
        Task<ResultDTO<PaginatedResponse<FundingProjectResponse>>> GetFundingProjects
            (ListRequest request, List<Guid>? categoryIds, List<ProjectStatus>? statusList, decimal? fromTarget, decimal? toTarget);
        Task<ResultDTO<FundingProjectResponse>> UpdateFundingProjectStatus(Guid id, ProjectStatus status, string? note);
        Task<ResultDTO<bool>> CheckProjectOwner(Guid projectId);
        Task<ResultDTO<List<FundingProjectResponse>>> GetTop3MostFundedOngoingFundingProject();
        Task<ResultDTO<PaginatedResponse<FundingProjectResponse>>> GetGameOwnerFundingProjects(ListRequest request, string? categoryName, ProjectStatus? status, decimal? fromTarget, decimal? toTarget);
        Task<ResultDTO<PaginatedResponse<FundingProjectResponse>>> GetBackerDonatedProjects(ListRequest request, string? categoryName, ProjectStatus? status, decimal? fromTarget, decimal? toTarget);
        Task DeleteFundingProject(Guid id);
    }
}
