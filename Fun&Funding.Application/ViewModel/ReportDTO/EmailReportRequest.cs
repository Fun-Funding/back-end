using Fun_Funding.Domain.Entity.NoSqlEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.ViewModel.ReportDTO
{
    public class EmailReportRequest
    {
        public Guid ReportId {  get; set; }
        public string? Content { get; set; }

    }
}
