using Fun_Funding.Application.Interfaces.IRepository;
using Fun_Funding.Application.IRepository;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Fun_Funding.Infrastructure.Persistence.Database;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class CartRepository : MongoBaseRepository<Cart>, ICartRepository
    {
        public CartRepository(MongoDBContext mongoDB) : base(mongoDB, "cart")
        {

        }
    }
}
