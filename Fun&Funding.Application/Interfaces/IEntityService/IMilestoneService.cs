using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.MilestoneDTO;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface IMilestoneService
    {
        Task<ResultDTO<MilestoneResponse>> CreateMilestone(AddMilestoneRequest request);
        Task<ResultDTO<List<MilestoneResponse>>> GetListLastestMilestone(bool? status, MilestoneFilter filter = MilestoneFilter.All);
        Task<ResultDTO<List<MilestoneResponse>>> GetMilestoneByVersionAndOrder(int? Order, int? Version);

        Task<ResultDTO<MilestoneResponse>> GetMilestoneById(Guid milestoneId, int? filter);
    }
}
