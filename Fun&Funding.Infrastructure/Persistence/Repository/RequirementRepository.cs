using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class RequirementRepository : BaseRepository<Requirement>, IRequirementRepository
    {
        public RequirementRepository(MyDbContext context) : base(context)
        {
        }
    }
}
