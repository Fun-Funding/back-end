using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.EmailDTO
{
    public class EmailRequest
    {
        public string ToEmail { get; set; }
        public EmailType EmailType { get; set; }
    }
}
