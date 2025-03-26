using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.FeedbackDTO
{
    public class FeedbackResponse
    {
        public string? Name {  get; set; }
        public string? Avatar { get; set; }
        public string? Content { get; set; }
    }
}
