using AutoMapper;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.IRepository;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CommentDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public CommentService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<List<CommentViewResponse>> GetAllComment()
        {
            try
            {
                var list = _unitOfWork.CommentRepository.GetAll();

                List<CommentViewResponse> comments = new List<CommentViewResponse>();

                foreach (var comment in list)
                {
                    // Fetch the user including the file (avatar)
                    var user = _unitOfWork.UserRepository.GetQueryable()
                        .Include(x => x.File) // Include the UserFile
                        .FirstOrDefault(x => x.Id == comment.UserID);

                    // Extract the avatar URL
                    var avatarUrl = user?.File?.URL;

                    comments.Add(new CommentViewResponse
                    {
                        Content = comment.Content,
                        CreateDate = comment.CreateDate,
                        UserName = user?.UserName,  // Ensure safe navigation
                        AvatarUrl = avatarUrl       // Use the extracted URL for avatar
                    });
                }

                return comments;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);

            }
        }

        public async Task<List<CommentViewResponse>> GetCommentsByFundingProject(Guid id)
        {
            try
            {
                var list = _unitOfWork.CommentRepository.GetAll().Where(c => c.FundingProjectId == id);
                List<CommentViewResponse> comments = new List<CommentViewResponse>();
                if (list != null)
                {
                    foreach (var comment in list)
                    {
                        // Fetch the user including the file (avatar)
                        var user = _unitOfWork.UserRepository.GetQueryable()
                            .Include(x => x.File) // Include the UserFile
                            .FirstOrDefault(x => x.Id == comment.UserID);

                        // Extract the avatar URL
                        var avatarUrl = user?.File?.URL ?? null;

                        comments.Add(new CommentViewResponse
                        {
                            Content = comment.Content,
                            CreateDate = comment.CreateDate,
                            UserName = user != null ? user?.UserName : "Anonymous",
                            AvatarUrl = avatarUrl,
                            UserId = user != null ? user.Id : null
                        });
                    }
                }

                return comments;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);

            }
        }
        public async Task<List<CommentViewResponse>> GetCommentsByMarketplaceProject(Guid id)
        {
            try
            {
                var list = _unitOfWork.CommentRepository.GetAll().Where(c => c.MarketplaceProjectId == id);
                List<CommentViewResponse> comments = new List<CommentViewResponse>();
                if(list != null)
                {
                    foreach (var comment in list)
                    {
                        // Fetch the user including the file (avatar)
                        var user = _unitOfWork.UserRepository.GetQueryable()
                            .Include(x => x.File) // Include the UserFile
                            .FirstOrDefault(x => x.Id == comment.UserID);

                        // Extract the avatar URL
                        var avatarUrl = user?.File?.URL ?? null;

                        comments.Add(new CommentViewResponse
                        {
                            Content = comment.Content,
                            CreateDate = comment.CreateDate,
                            UserName = user != null ? user?.UserName : "Anonymous",
                            AvatarUrl = avatarUrl,
                            UserId = user != null ? user.Id : null
                        });
                    }
                }

                return comments;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);

            }
        }
        public async Task<ResultDTO<Comment>> CommentFundingProject(CommentRequest request)
        {
            try
            {
                var user = await _userService.GetUserInfo();
                User exitUser = _mapper.Map<User>(user._data);
                var project = await _unitOfWork.FundingProjectRepository.GetAsync(x => x.Id.Equals(request.ProjectId));
                if (user is null)
                {
                    return ResultDTO<Comment>.Fail("User is null");
                }
                if (project is null)
                {
                    return ResultDTO<Comment>.Fail("Project can not found");
                }

                // add new comment
                Comment newComment = new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = request.Content,
                    CreateDate = DateTime.Now,
                    FundingProjectId = project.Id,
                    UserID = exitUser.Id,
                    IsDelete = false,
                };
                _unitOfWork.CommentRepository.Create(newComment);

                    // NOTIFICATION
                    // 1. get recipientsIds
                    List<Guid> recipientsId = new List<Guid>();
                    recipientsId.Add(project.UserId); // project owner
                                                      // 2. initiate new Notification object
                    var notification = new Notification
                    {
                        Id = new Guid(),
                        Date = DateTime.Now,
                        Message = $"commented on project <b>{project.Name}</b>",
                        NotificationType = NotificationType.FundingProjectInteraction,
                        Actor = new { user._data.Id, user._data.UserName, user._data.Avatar },
                        ObjectId = project.Id,
                    };
                    // 3. send noti
                    await _notificationService.SendNotification(notification, recipientsId);


                return ResultDTO<Comment>.Success(newComment, "Successfully Add Comment");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<ResultDTO<Comment>> CommentMarketplaceProject(CommentRequest request)
        {
            try
            {
                var user = await _userService.GetUserInfo();
                User exitUser = _mapper.Map<User>(user._data);
                var project = await _unitOfWork.MarketplaceRepository
                    .GetQueryable()
                    .Include(m => m.FundingProject)
                    .FirstOrDefaultAsync(x => x.Id.Equals(request.ProjectId));
                if (user is null)
                {
                    return ResultDTO<Comment>.Fail("User is null");
                }
                if (project is null)
                {
                    return ResultDTO<Comment>.Fail("Project can not found");
                }

                // add new comment
                Comment newComment = new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = request.Content,
                    CreateDate = DateTime.Now,
                    MarketplaceProjectId = project.Id,
                    UserID = exitUser.Id,
                    IsDelete = false,
                };
                _unitOfWork.CommentRepository.Create(newComment);

                    // NOTIFICATION
                    // 1. get recipientsIds
                    List<Guid> recipientsId = new List<Guid>();
                    recipientsId.Add(project.FundingProject.UserId); // project owner
                    // 2. initiate new Notification object
                    var notification = new Notification
                    {
                        Id = new Guid(),
                        Date = DateTime.Now,
                        Message = $"commented on project <b>{project.Name}</b>",
                        NotificationType = NotificationType.MarketplaceProjectInteraction,
                        Actor = new { user._data.Id, user._data.UserName, user._data.Avatar },
                        ObjectId = project.Id,
                    };
                    // 3. send noti
                    await _notificationService.SendNotification(notification, recipientsId);

                return ResultDTO<Comment>.Success(newComment, "Successfully Add Comment");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<ResultDTO<Comment>> DeleteComment(Guid id)
        {
            //check comment id
            var extiedComment = _unitOfWork.CommentRepository.Get(x => x.Id == id);
            var user = await _userService.GetUserInfo();
            User exitUser = _mapper.Map<User>(user._data);
            if (extiedComment is null)
            {
                return ResultDTO<Comment>.Fail("can not find any comment");
            }
            if (!extiedComment.UserID.Equals(user._data.Id))
            {
                return ResultDTO<Comment>.Fail("user are not authorized to do this action");

            }
            try
            {
                var updateComment = Builders<Comment>.Update.Set(x => x.IsDelete, true);
                _unitOfWork.CommentRepository.SoftRemove(x => x.Id == extiedComment.Id, updateComment);
                return ResultDTO<Comment>.Success(extiedComment, "Delete Successfull");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


    }
}
