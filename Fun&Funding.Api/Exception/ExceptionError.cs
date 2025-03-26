namespace Fun_Funding.Api.Exception
{
    public class ExceptionError : System.Exception
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public ExceptionError(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
