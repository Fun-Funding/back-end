using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.CommentDTO
{
    public class CommentRequest
    {
        public string? Content { get; set; }
        public Guid ProjectId { get; set; }
    }
}
