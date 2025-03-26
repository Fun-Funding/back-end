using AutoMapper;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.IRepository;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.FollowDTO;
using Fun_Funding.Application.ViewModel.LikeDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class LikeService : ILikeService
    {
        private readonly IUserService _userService;
        private readonly IMarketplaceService _marketplaceService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public LikeService(IUserService userService, IMarketplaceService marketplaceService, IMapper mapper, IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _userService = userService;
            _marketplaceService = marketplaceService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<ResultDTO<Like>> CheckUserLike(Guid projectId)
        {
            try
            {
                // Get current user
                var user = await _userService.GetUserInfo();
                User exitUser = _mapper.Map<User>(user._data);
                if (exitUser == null)
                {
                    return ResultDTO<Like>.Fail("Cannot find user");
                }

              
                // Check for like in funding project
                var fundingLike = await _unitOfWork.FundingProjectRepository.GetByIdAsync(projectId);
                if (fundingLike != null)
                {
                    var isLike = _unitOfWork.LikeRepository.Get(x => x.ProjectId == fundingLike.Id && x.UserId == exitUser.Id);
                    if (isLike != null) return ResultDTO<Like>.Success(isLike,"user has like this funding project");
                }

                // Check for like in marketplace project
                var marketplaceProject = await _unitOfWork.MarketplaceRepository.GetByIdAsync(projectId);
                if (marketplaceProject != null)
                {
                    var isLike = _unitOfWork.LikeRepository.Get(x => x.ProjectId == marketplaceProject.Id && x.UserId == exitUser.Id);
                    if (isLike != null) return ResultDTO<Like>.Success(isLike, "user has like this marketplace project");
                }

                return ResultDTO<Like>.Fail("User has not liked any project", 404);
            }
            catch (Exception ex)
            {
                return ResultDTO<Like>.Fail($"Error: {ex.Message}");
            }
        }
        public async Task<List<Like>> GetAll()
        {
            try
            {
                var list = _unitOfWork.LikeRepository.GetAll();
                var result = list.Where(x => x.IsDelete == false).ToList();
                return list.ToList();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);

            }
        }
        public async Task<ResultDTO<LikeResponse>> LikeFundingProject(LikeRequest likeRequest)
        {
            try
            {
                var user = await _userService.GetUserInfo();
                User exitUser = _mapper.Map<User>(user._data);
                var project = await _unitOfWork.FundingProjectRepository.GetAsync(x => x.Id.Equals(likeRequest.ProjectId));
                if (user is null)
                {
                    return ResultDTO<LikeResponse>.Fail("User is null");
                }
                if (project is null)
                {
                    return ResultDTO<LikeResponse>.Fail("Project can not found");
                }
                //check if the user and project already liked 
                var getLikedProjects = _unitOfWork.LikeRepository.Get(x => x.ProjectId.Equals(project.Id) && x.UserId.Equals(exitUser.Id));


                if (getLikedProjects == null)
                {
                    //liked a project
                    Like newLikeProject = new Like
                    {
                        ProjectId = likeRequest.ProjectId,
                        UserId = exitUser.Id,
                        IsLike = true,
                        CreateDate = DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsDelete = false,
                    };
                    _unitOfWork.LikeRepository.Create(newLikeProject);

                    // NOTIFICATION
                    // 1. get recipientsIds
                    List<Guid> recipientsId = new List<Guid>();
                    recipientsId.Add(project.UserId); // project owner
                                                      // 2. initiate new Notification object
                    var notification = new Notification
                    {
                        Id = new Guid(),
                        Date = DateTime.Now,
                        Message = $"liked project <b>{project.Name}</b>",
                        NotificationType = NotificationType.FundingProjectInteraction,
                        Actor = new { user._data.Id, user._data.UserName, user._data.Avatar },
                        ObjectId = project.Id,
                    };
                    // 3. send noti
                    await _notificationService.SendNotification(notification, recipientsId);

                    return ResultDTO<LikeResponse>.Success(new LikeResponse { ProjectId = newLikeProject.ProjectId, UserID = newLikeProject.UserId }, "Succesfull like the project");
                }
                else
                {
                    if (getLikedProjects.IsLike == false && getLikedProjects.IsDelete)
                    {
                        var updateDefinition = Builders<Like>.Update.Set(x => x.IsLike, true).Set(x => x.IsDelete, false);
                        _unitOfWork.LikeRepository.Update(x => x.Id == getLikedProjects.Id, updateDefinition);

                                // NOTIFICATION
                                // 1. get recipientsIds
                                List<Guid> recipientsId = new List<Guid>();
                                recipientsId.Add(project.UserId); // project owner
                                // 2. initiate new Notification object
                                var notification = new Notification
                                {
                                    Id = new Guid(),
                                    Date = DateTime.Now,
                                    Message = $"liked project <b>{project.Name}</b>",
                                    NotificationType = NotificationType.FundingProjectInteraction,
                                    Actor = new { user._data.Id, user._data.UserName, user._data.Avatar },
                                    ObjectId = project.Id,
                                };
                                // 3. send noti
                                await _notificationService.SendNotification(notification, recipientsId);

                        return ResultDTO<LikeResponse>.Success(new LikeResponse { ProjectId = likeRequest.ProjectId, UserID = exitUser.Id }, "Succesfull like the project");
                    }
                    if (getLikedProjects.IsLike && getLikedProjects.IsDelete == false) //isLike == true ? "dislike" : "liked"
                    {
                        var update = Builders<Like>.Update.Set(l => l.IsDelete, true).Set(x => x.IsLike, false);
                        _unitOfWork.LikeRepository.SoftRemove(l => l.Id == getLikedProjects.Id, update);
                        return ResultDTO<LikeResponse>.Success(new LikeResponse { ProjectId = likeRequest.ProjectId, UserID = exitUser.Id }, "Succesfull dislike the project");
                    }
                }

                return ResultDTO<LikeResponse>.Fail("some thing wrong : error ");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public async Task<ResultDTO<Like>> LikeMarketplaceProject(LikeRequest likeRequest)
        {
            try
            {
                var user = await _userService.GetUserInfo();
                User exitUser = _mapper.Map<User>(user._data);
                var project = await _unitOfWork.MarketplaceRepository
                    .GetQueryable()
                    .Include(m => m.FundingProject)
                        .ThenInclude(fp => fp.User)
                    .FirstOrDefaultAsync(m => m.Id.Equals(likeRequest.ProjectId));
                if (user is null)
                {
                    return ResultDTO<Like>.Fail("User is null");
                }
                if (project is null)
                {
                    return ResultDTO<Like>.Fail("Project can not found");
                }
                //check if the user and project already liked 
                var getLikedProjects = _unitOfWork.LikeRepository.Get(x => x.ProjectId.Equals(project.Id) && x.UserId.Equals(exitUser.Id));


                if (getLikedProjects == null)
                {
                    //liked a project
                    Like newLikeProject = new Like
                    {
                        ProjectId = likeRequest.ProjectId,
                        UserId = exitUser.Id,
                        IsLike = true,
                        CreateDate = DateTime.Now,
                        Id = Guid.NewGuid(),
                        IsDelete = false,
                    };
                    _unitOfWork.LikeRepository.Create(newLikeProject);

                    // NOTIFICATION
                    // 1. get recipientsIds
                    List<Guid> recipientsId = new List<Guid>();
                    recipientsId.Add(project.FundingProject.UserId); // project owner
                                                                     // 2. initiate new Notification object
                    var notification = new Notification
                    {
                        Id = new Guid(),
                        Date = DateTime.Now,
                        Message = $"liked project <b>{project.Name}</b>",
                        NotificationType = NotificationType.MarketplaceProjectInteraction,
                        Actor = new { user._data.Id, user._data.UserName, user._data.Avatar },
                        ObjectId = project.Id,
                    };
                    // 3. send noti
                    await _notificationService.SendNotification(notification, recipientsId);

                    return ResultDTO<Like>.Success(newLikeProject, "Succesfull like the project");
                }
                else
                {
                    if (getLikedProjects.IsLike == false && getLikedProjects.IsDelete)
                    {
                        var updateDefinition = Builders<Like>.Update.Set(x => x.IsLike, true).Set(x => x.IsDelete, false);
                        _unitOfWork.LikeRepository.Update(x => x.Id == getLikedProjects.Id, updateDefinition);

                                // NOTIFICATION
                                // 1. get recipientsIds
                                List<Guid> recipientsId = new List<Guid>();
                                recipientsId.Add(project.FundingProject.UserId); // project owner
                                // 2. initiate new Notification object
                                var notification = new Notification
                                {
                                    Id = new Guid(),
                                    Date = DateTime.Now,
                                    Message = $"liked project <b>{project.Name}</b>",
                                    NotificationType = NotificationType.MarketplaceProjectInteraction,
                                    Actor = new { user._data.Id, user._data.UserName, user._data.Avatar },
                                    ObjectId = project.Id,
                                };
                                // 3. send noti
                                await _notificationService.SendNotification(notification, recipientsId);

                        return ResultDTO<Like>.Success(getLikedProjects, "Succesfull like the project");
                    }
                    if (getLikedProjects.IsLike && getLikedProjects.IsDelete == false) //isLike == true ? "dislike" : "liked"
                    {
                        var update = Builders<Like>.Update.Set(l => l.IsDelete, true).Set(x => x.IsLike, false);
                        _unitOfWork.LikeRepository.SoftRemove(l => l.Id == getLikedProjects.Id, update);
                        return ResultDTO<Like>.Success(getLikedProjects, "Succesfull dislike the project");
                    }
                }

                return ResultDTO<Like>.Fail("some thing wrong : error ");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResultDTO<MarketplaceLikeNumbers>> NumberOfMarketplaceLike()
        {
            var user = await _userService.GetUserInfo();
            if (user is null)
            {
                return ResultDTO<MarketplaceLikeNumbers>.Fail("No user found");
            }
            User exitUser = _mapper.Map<User>(user._data);
            try
            {
                var list = _unitOfWork.LikeRepository.GetList(x=>x.UserId == exitUser.Id);
                var response = new MarketplaceLikeNumbers();
                response.LikeCount = list.Count;
                response.marketplaceProjectResponses = new List<ViewModel.MarketplaceProjectDTO.MarketplaceProjectInfoResponse>();
                foreach (var item in list) {
                    var likeMarketplace = await _marketplaceService.GetMarketplaceProjectById(item.ProjectId);
                    if (likeMarketplace._data != null)
                    {
                        response.marketplaceProjectResponses!.Add(likeMarketplace._data);
                    }
                }
                return ResultDTO<MarketplaceLikeNumbers>.Success(response, "Successful get marketplace like");

            }
            catch (Exception ex)
            {
                return ResultDTO<MarketplaceLikeNumbers>.Fail(ex.Message);
            }
        }
    }
}
