using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface IBackgroundProcessService
    {
        Task UpdateFundingStatus();
        Task RefundFundingBackers(Guid id);
        Task UpdateProjectMilestoneStatus();
    }
}
