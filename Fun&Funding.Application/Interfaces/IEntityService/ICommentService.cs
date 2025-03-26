using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CommentDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface ICommentService
    {
        Task<List<CommentViewResponse>> GetAllComment();
        Task<ResultDTO<Comment>> CommentFundingProject(CommentRequest request);
        Task<ResultDTO<Comment>> CommentMarketplaceProject(CommentRequest request);
        Task<List<CommentViewResponse>> GetCommentsByFundingProject(Guid id);
        Task<List<CommentViewResponse>> GetCommentsByMarketplaceProject(Guid id);
        Task<ResultDTO<Comment>> DeleteComment(Guid id);
    }
}
