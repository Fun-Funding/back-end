using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(MyDbContext context) : base(context)
        {
        }
    }
}
