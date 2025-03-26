using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.CustomUserIdProvider
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            //Console.WriteLine($"UserId here: + {connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //return connection.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            //return "34AA3C57-B91F-4387-97C2-F68B50366B76";
            //return _claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        }
    }
}
