using Fun_Funding.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Interfaces.IExternalServices
{
    public interface ITokenGenerator
    {
        public string GenerateToken(User user, IList<string> userRole);
    }
}
