using Fun_Funding.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Interfaces.IExternalServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, EmailType type);
        Task SendReportAsync(string toEmail, string projectName, string userName, DateTime reportedDate, string content, List<string> reason);
        Task SendUserReportAsync(string toEmail, string userName, DateTime reportedDate, string content, List<string> reason);
        Task SendMilestoneAsync(string toEmail,string projectName, string milestoneName, string ownerName, string? newStatus,int? timeSpan, DateTime reportedDate, EmailType type);
        
    }
}
