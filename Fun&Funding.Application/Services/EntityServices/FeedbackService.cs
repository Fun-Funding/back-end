using AutoMapper;
using Fun_Funding.Application.Interfaces.IEntityService;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.FeedbackDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<ResultDTO<Feedback>> ApprovedById(Guid id)
        {
            try
            {

                var feedback = GetFeedbackById(id).Result._data;
                if (feedback.Status)
                {
                    _unitOfWork.FeedbackRepository.Update(x => x.Id == feedback.Id, Builders<Feedback>.Update.Set(x => x.Status, false));
                    await _unitOfWork.CommitAsync();
                }
                else
                {
                    var listFeedback = _unitOfWork.FeedbackRepository.GetList(x => x.Status);
                    if (listFeedback.Count >= 4)
                    {
                        return ResultDTO<Feedback>.Fail("Maximum approved feedback, please change status");
                    }
                    _unitOfWork.FeedbackRepository.Update(x => x.Id == feedback.Id, Builders<Feedback>.Update.Set(x => x.Status, true));
                    await _unitOfWork.CommitAsync();
                }
                var resFeedback = GetFeedbackById(id).Result._data;
                return ResultDTO<Feedback>.Success(resFeedback, "Successfull Approved Query");

            }
            catch (Exception ex)
            {
                return ResultDTO<Feedback>.Fail($"somethings wrong: {ex.Message}");
            }
        }

        public async Task<ResultDTO<Feedback>> CreateFeedBack(FeedbackRequest request)
        {
            try
            {
                var user = await _userService.GetUserInfo();
                User existUser = _mapper.Map<User>(user._data);
                if (existUser is null)
                {
                    return ResultDTO<Feedback>.Fail("No User found");
                }
                var feedback = new Feedback
                {
                    Id = Guid.NewGuid(),
                    Content = request.Content,
                    CreateDate = DateTime.Now,
                    IsDelete = false,
                    UserID = existUser.Id,
                    Status = false,
                };
                _unitOfWork.FeedbackRepository.Create(feedback);
                await _unitOfWork.CommitAsync();
                return ResultDTO<Feedback>.Success(feedback, "successfull create feedback");
            }
            catch (Exception ex)
            {
                return ResultDTO<Feedback>.Fail($"something wrongs: {ex.Message}");
            }
        }

        public async Task<ResultDTO<PaginatedResponse<Feedback>>> GetAllFeedback(ListRequest request)
        {
            try
            {
                var list = _unitOfWork.FeedbackRepository.GetAllPaged(request);
                return ResultDTO<PaginatedResponse<Feedback>>.Success(list, "Successfull querry");
            }
            catch (Exception ex)
            {
                return ResultDTO<PaginatedResponse<Feedback>>.Fail("something wrong!");
            }
        }

        public async Task<ResultDTO<Feedback>> GetFeedbackById(Guid id)
        {
            try
            {
                var exitedFeedback = _unitOfWork.FeedbackRepository.Get(x => x.Id == id);
                if (exitedFeedback == null) return ResultDTO<Feedback>.Fail($"not found any feedback");
                return ResultDTO<Feedback>.Success(exitedFeedback, "Successfull get feedback");
            }
            catch (Exception ex)
            {
                return ResultDTO<Feedback>.Fail($"somethings wrong: {ex.Message}");
            }
        }

        public async Task<ResultDTO<List<FeedbackResponse>>> GetTop4Feedback()
        {
            try
            {
                var listFeedback = _unitOfWork.FeedbackRepository.GetList(x => x.Status);
                var responseList = new List<FeedbackResponse>();
                foreach (var feedback in listFeedback)
                {
                    var user = await _unitOfWork.UserRepository.GetByIdAsync(feedback.UserID);
                    responseList.Add(new FeedbackResponse
                    {
                        Name = user?.FullName ?? "Anonymous",
                        Avatar = user?.File?.URL ?? null,
                        Content = feedback.Content ?? null
                    });
                }

                return ResultDTO<List<FeedbackResponse>>.Success(responseList, "Successfull querry");
            }
            catch (Exception ex)
            {
                return ResultDTO<List<FeedbackResponse>>.Fail("something wrong!");
            }
        }
    }
}
