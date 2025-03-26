using Fun_Funding.Domain.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.UserDTO
{
    public class UserFileRequest
    {
        public string Name { get; set; }
        public IFormFile URL { get; set; }
    }
}
