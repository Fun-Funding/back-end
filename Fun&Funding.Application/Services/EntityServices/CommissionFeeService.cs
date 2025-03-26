using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.CommissionDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System.Linq.Expressions;
using System.Net;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class CommissionFeeService : ICommissionFeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommissionFeeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultDTO<CommissionFeeResponse>> CreateCommissionFee(CommissionFeeAddRequest request)
        {
            try
            {
                if (request.Rate < 0 || request.Rate > 1)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Invalid Rate");
                }
                else
                {
                    var commission = _mapper.Map<CommissionFee>(request);
                    commission.CreatedDate = commission.UpdateDate = DateTime.Now;
                    _unitOfWork.CommissionFeeRepository.Add(commission);
                    await _unitOfWork.CommitAsync();

                    var response = _mapper.Map<CommissionFeeResponse>(commission);

                    return new ResultDTO<CommissionFeeResponse>(true, ["Create successfully."], response, (int)HttpStatusCode.Created);
                }
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

        public ResultDTO<CommissionFeeResponse> GetAppliedCommissionFee(CommissionType type)
        {
            try
            {
                var commissionFee = _unitOfWork.CommissionFeeRepository.GetAppliedCommissionFeeByType(type);

                if (commissionFee != null)
                {
                    var response = _mapper.Map<CommissionFeeResponse>(commissionFee);

                    return ResultDTO<CommissionFeeResponse>.Success(response);
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "No Commission Fee in Database.");
                }

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

        public async Task<ResultDTO<PaginatedResponse<CommissionFeeResponse>>>
            GetCommissionFees(ListRequest request, CommissionType? type)
        {
            try
            {
                Expression<Func<CommissionFee, bool>> filter = null;
                Expression<Func<CommissionFee, object>> orderBy = c => c.UpdateDate;

                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    filter = c => c.Version.ToLower().Contains(request.SearchValue.ToLower());
                }

                if (type.HasValue)
                {
                    filter = c => c.CommissionType == type;
                }

                var list = await _unitOfWork.CommissionFeeRepository.GetAllAsync(
                    filter: filter,
                    orderBy: orderBy,
                    isAscending: request.IsAscending.Value,
                    pageIndex: request.PageIndex,
                    pageSize: request.PageSize);

                if (list != null)
                {
                    var totalItems = _unitOfWork.CommissionFeeRepository.GetAll(filter).Count();
                    var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);

                    IEnumerable<CommissionFeeResponse> commissionFees = _mapper.Map<IEnumerable<CommissionFeeResponse>>(list);

                    PaginatedResponse<CommissionFeeResponse> response = new PaginatedResponse<CommissionFeeResponse>
                    {
                        PageSize = request.PageSize.Value,
                        PageIndex = request.PageIndex.Value,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = commissionFees
                    };

                    return ResultDTO<PaginatedResponse<CommissionFeeResponse>>.Success(response);
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Commission Fee Not Found.");
                }
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

        public ResultDTO<List<CommissionFeeResponse>> GetListAppliedCommissionFee()
        {
            try
            {
                var commissionFees = _unitOfWork.CommissionFeeRepository.GetQueryable()
                     .OrderByDescending(c => c.UpdateDate) // Order by UpdateDate in descending order
                     .AsEnumerable()                        // Switch to in-memory processing
                     .DistinctBy(c => c.CommissionType)     // Get distinct records by CommissionType
                     .Take(2)                               // Take the top 2 records
                     .ToList();

                if (commissionFees != null)
                {
                    var response = _mapper.Map<List<CommissionFeeResponse>>(commissionFees);

                    return ResultDTO<List<CommissionFeeResponse>>.Success(response);
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "No Commission Fee in Database.");
                }

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

        public async Task<ResultDTO<CommissionFeeResponse>> UpdateCommsisionFee(Guid id, CommissionFeeUpdateRequest request)
        {
            try
            {
                if (request.Rate < 0 || request.Rate > 1)
                {
                    throw new ExceptionError((int)HttpStatusCode.BadRequest, "Invalid Rate");
                }
                else
                {
                    var commission = await _unitOfWork.CommissionFeeRepository.GetByIdAsync(id);

                    if (commission != null)
                    {
                        commission.Id = new Guid();
                        commission.Rate = request.Rate;
                        commission.UpdateDate = DateTime.Now;

                        _unitOfWork.CommissionFeeRepository.Add(commission);
                        await _unitOfWork.CommitAsync();

                        var response = _mapper.Map<CommissionFeeResponse>(commission);

                        return ResultDTO<CommissionFeeResponse>.Success(response);
                    }
                    else
                    {
                        throw new ExceptionError((int)HttpStatusCode.NotFound, "Commission Fee not found.");
                    }
                }
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
