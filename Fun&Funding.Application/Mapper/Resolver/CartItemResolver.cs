using AutoMapper;
using Fun_Funding.Application.ViewModel.CartDTO;
using Fun_Funding.Application.ViewModel.MarketplaceProjectDTO;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Mapper.Resolver
{
    public class CartItemResolver : IValueResolver<Cart, CartInfoResponse, List<ItemInfoResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CartItemResolver(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public List<ItemInfoResponse> Resolve(Cart source, CartInfoResponse destination, List<ItemInfoResponse> destMember, ResolutionContext context)
        {
            var items = new List<ItemInfoResponse>();

            var projectIds = new HashSet<Guid>();

            foreach (var bsonItem in source.Items)
            {
                if (bsonItem.TryGetValue("marketplaceProjectId", out BsonValue projectIdValue) && projectIdValue.IsGuid)
                {
                    projectIds.Add(projectIdValue.AsGuid);
                }
            }

            var projects = _unitOfWork.MarketplaceRepository.GetQueryable()
                .AsNoTracking()
                .Include(p => p.MarketplaceFiles)
                .Where(p => projectIds.Contains(p.Id))
                .ToList();

            var projectLookup = projects.ToDictionary(p => p.Id);

            foreach (var bsonItem in source.Items)
            {
                if (bsonItem.TryGetValue("marketplaceProjectId", out BsonValue projectIdValue) && projectIdValue.IsGuid)
                {
                    Guid projectId = projectIdValue.AsGuid;

                    if (projectLookup.TryGetValue(projectId, out MarketplaceProject projectInfo))
                    {
                        items.Add(new ItemInfoResponse
                        {
                            MarketplaceProject = _mapper.Map<MarketplaceProjectInfoResponse>(projectInfo),
                            CreatedDate = bsonItem.TryGetValue("createdDate", out BsonValue createdDateValue) && createdDateValue.IsValidDateTime
                                ? createdDateValue.ToLocalTime()
                                : null,
                        });
                    }
                }
            }

            return items;
        }

    }

}
