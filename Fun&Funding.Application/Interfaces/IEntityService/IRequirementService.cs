using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.RequirementDTO;
using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface IRequirementService
    {
        Task<ResultDTO<RequirementResponse>> GetRequirementById(Guid id);
        Task<ResultDTO<RequirementResponse>> CreateNewRequirement(RequirementRequest request);
        Task<ResultDTO<RequirementResponse>> UpdateRequirement(UpdateRequirement request);
    }
}
