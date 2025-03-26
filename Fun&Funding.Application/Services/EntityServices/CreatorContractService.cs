using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.IRepository;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CreatorContractDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class CreatorContractService : ICreatorContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public CreatorContractService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }
        public async Task<ResultDTO<CreatorContract>> CreateContract()
        {
            try
            {
                var user = _userService.GetUserInfo().Result;
                User exitUser = _mapper.Map<User>(user._data);
                if (user is null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "User not found");
                }
                
                var sampleContract = _unitOfWork.CreatorContractRepository.GetQueryable().FirstOrDefault(sc => sc.ContractType == ContractType.Sample);
                var creatorContract = new CreatorContract
                {
                    UserId = exitUser.Id,
                    Policies = sampleContract.Policies,
                    ContractType = ContractType.CreatorPolicies
                };
                _unitOfWork.CreatorContractRepository.Create(creatorContract);
                return ResultDTO<CreatorContract>.Success(creatorContract);
            }
            catch (Exception ex)
            {
                if (ex is ExceptionError exceptionError)
                {
                    throw exceptionError;
                }
                throw new ExceptionError((int)HttpStatusCode.InternalServerError, ex.Message);
            }

        }
    }
}
