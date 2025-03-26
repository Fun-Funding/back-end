using AutoMapper;
using Fun_Funding.Application.ExceptionHandler;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.DigitalKeyDTO;
using Fun_Funding.Application.ViewModel.OrderDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Services.EntityServices
{
    public class DigitalKeyService : IDigitalKeyService
    {
        private static readonly Random random = new Random();
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DigitalKeyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public string GenerateGameKey()
        {
            StringBuilder keyBuilder = new StringBuilder();
            string key;
            bool isDuplicate;

            do
            {
                keyBuilder.Clear();
                for (int i = 0; i < 4; i++)
                {
                    if (i > 0) keyBuilder.Append('-');
                    for (int j = 0; j < 5; j++)
                    {
                        keyBuilder.Append(chars[random.Next(chars.Length)]);
                    }
                }
                key = keyBuilder.ToString();
                isDuplicate = _unitOfWork.DigitalKeyRepository
                             .GetQueryable()
                             .Any(k => k.KeyString == key);

            } while (isDuplicate);

            return key;
        }

        public async Task<ResultDTO<string>> VerifyDigitalKey(string key, string projectName)
        {
            try
            {
                MarketplaceProject marketplaceProject = _unitOfWork.MarketplaceRepository.GetQueryable()
                .Where(p => p.Name == (projectName ?? string.Empty))
                .SingleOrDefault();
                if (marketplaceProject == null) {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Cannot find project name.");
                }
                DigitalKey digitalKey = _unitOfWork.DigitalKeyRepository.GetQueryable().Where(k => k.KeyString == key && k.MarketplaceProject.Id == marketplaceProject.Id).SingleOrDefault();
                if (digitalKey != null)
                {
                    if(digitalKey.Status != KeyStatus.ACTIVE)
                    {
                        throw new ExceptionError((int)HttpStatusCode.NotFound, "Game Key Has Been Used.");
                    }
                    digitalKey.Status = KeyStatus.EXPIRED;
                    digitalKey.ExpiredDate = DateTime.Now;
                    _unitOfWork.DigitalKeyRepository.Update(digitalKey);
                    await _unitOfWork.CommitAsync();
                    return ResultDTO<string>.Success("", "Verify Key Successfully!");
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Game Key Not Found For The Corresponding Project.");
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

        public async Task<ResultDTO<DigitalKeyInfoResponse>> GenerateTestKey(Guid marketplaceProjectId)
        {
            try
            {
                DigitalKey digitalKey = new DigitalKey
                {
                    Id = Guid.NewGuid(),
                    KeyString = GenerateGameKey(),
                    Status = KeyStatus.ACTIVE,
                    CreatedDate = DateTime.Now,
                    MarketplaceProject = _unitOfWork.MarketplaceRepository.GetById(marketplaceProjectId)
                };
                _unitOfWork.DigitalKeyRepository.Add(digitalKey);
                await _unitOfWork.CommitAsync();
                DigitalKey addedDigitalKey = _unitOfWork.DigitalKeyRepository.GetById(digitalKey.Id);
                DigitalKeyInfoResponse digitalKeyResponse = _mapper.Map<DigitalKeyInfoResponse>(addedDigitalKey);
                return ResultDTO<DigitalKeyInfoResponse>.Success(digitalKeyResponse, "Generate Test Key Successfully!");
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

        public async Task<ResultDTO<DigitalKeyInfoResponse>> GetDigitalKeyById(Guid id)
        {
            try
            {
                DigitalKey addedDigitalKey = await _unitOfWork.DigitalKeyRepository.GetByIdAsync(id);
                if (addedDigitalKey == null)
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Game Key Not Found.");
                }
                DigitalKeyInfoResponse digitalKeyResponse = _mapper.Map<DigitalKeyInfoResponse>(addedDigitalKey);
                return ResultDTO<DigitalKeyInfoResponse>.Success(digitalKeyResponse, "Game Key Found!");
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

        public async Task<ResultDTO<PaginatedResponse<DigitalKeyInfoResponse>>> GetAllDigitalKey(ListRequest request)
        {
            try
            {
                Expression<Func<DigitalKey, bool>> filter = null;
                Expression<Func<DigitalKey, object>> orderBy = u => u.CreatedDate;

                if (!string.IsNullOrEmpty(request.SearchValue))
                {
                    string searchLower = request.SearchValue.ToLower();
                    filter = u =>
                        u.MarketplaceProject.Name != null && u.MarketplaceProject.Name.ToLower().Contains(searchLower);
                }

                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    switch (request.OrderBy.ToLower())
                    {
                        case "ExpireDate":
                            orderBy = u => u.ExpiredDate;
                            break;
                        default:
                            orderBy = u => u.CreatedDate;
                            break;
                    }
                }

                if (request.From != null && request.To != null)
                {
                    DateTime fromDate = (DateTime)request.From;
                    DateTime toDate = (DateTime)request.To;
                    filter = c => c.ExpiredDate >= fromDate && c.ExpiredDate <= toDate;
                }
                else if (request.From != null)
                {
                    DateTime fromDate = (DateTime)request.From;
                    filter = c => c.ExpiredDate >= fromDate;
                }
                else if (request.To != null)
                {
                    DateTime toDate = (DateTime)request.To;
                    filter = c => c.ExpiredDate <= toDate;
                }

                var list = await _unitOfWork.DigitalKeyRepository.GetAllAsync(
                       filter: filter,
                       orderBy: orderBy,
                       isAscending: request.IsAscending.Value,
                       pageIndex: request.PageIndex,
                       pageSize: request.PageSize,
                       includeProperties: "MarketplaceProject");

                if (list != null && list.Count() > 0)
                {
                    var totalItems = _unitOfWork.DigitalKeyRepository.GetAll(filter).Count();
                    var totalPages = (int)Math.Ceiling((double)totalItems / (int)request.PageSize);
                    IEnumerable<DigitalKeyInfoResponse> orders = _mapper.Map<IEnumerable<DigitalKeyInfoResponse>>(list);

                    PaginatedResponse<DigitalKeyInfoResponse> response = new PaginatedResponse<DigitalKeyInfoResponse>
                    {
                        PageSize = request.PageSize.Value,
                        PageIndex = request.PageIndex.Value,
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        Items = orders
                    };

                    return ResultDTO<PaginatedResponse<DigitalKeyInfoResponse>>.Success(response, "Digital Key Found!");
                }
                else
                {
                    throw new ExceptionError((int)HttpStatusCode.NotFound, "Digital Key Not Found.");
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
