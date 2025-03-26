namespace Fun_Funding.Application.ExceptionHandler
{
    public class ExceptionError : System.Exception
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public ExceptionError(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
