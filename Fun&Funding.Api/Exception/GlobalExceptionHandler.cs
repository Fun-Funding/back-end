using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fun_Funding.Api.Exception
{
    public class GlobalExceptionHandler : ExceptionFilterAttribute
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public override void OnException(ExceptionContext context)
        {
            // log ExceptionError in Application Project
            var exceptionError = context.Exception as Fun_Funding.Application.ExceptionHandler.ExceptionError;

            // Nếu là ExceptionError, trả về StatusCode và Message
            if (exceptionError != null)
            {
                context.Result = new JsonResult(new
                {
                    StatusCode = exceptionError.StatusCode,
                    Message = exceptionError.Message
                });
                context.HttpContext.Response.StatusCode = exceptionError.StatusCode;
            }
            else
            {
                // Nếu không phải ExceptionError, trả về StatusCode 500 và Message
                context.Result = new JsonResult(new
                {
                    StatusCode = 500,
                    Message = context.Exception.Message
                });
                context.HttpContext.Response.StatusCode = 500;
            }
        }
    }
}
