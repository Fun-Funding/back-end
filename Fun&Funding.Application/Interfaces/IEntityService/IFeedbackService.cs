using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.FeedbackDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Interfaces.IEntityService
{
    public interface IFeedbackService
    {
        public Task<ResultDTO<PaginatedResponse<Feedback>>> GetAllFeedback(ListRequest request);
        public Task<ResultDTO<Feedback>> GetFeedbackById(Guid id);
        public Task<ResultDTO<Feedback>> ApprovedById(Guid id);
        public Task<ResultDTO<Feedback>> CreateFeedBack(FeedbackRequest request);
        public Task<ResultDTO<List<FeedbackResponse>>> GetTop4Feedback();
    }
}
