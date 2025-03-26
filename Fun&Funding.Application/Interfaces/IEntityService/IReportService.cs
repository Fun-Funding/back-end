using Fun_Funding.Application.ViewModel;
using Fun_Funding.Application.ViewModel.ReportDTO;
using Fun_Funding.Domain.Entity.NoSqlEntities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.IService
{
    public interface IReportService
    {
        Task<ResultDTO<ViolentReport>> CreateReportRequest(ReportRequest request);
        Task<ResultDTO<PaginatedResponse<ViolentReport>>> GetAllReport(ListRequest request);
        Task<ResultDTO<ViolentReport>> UpdateHandleReport(Guid id);
        Task<ResultDTO<string>> SendReportedEmail(EmailReportRequest request);

    }
}
