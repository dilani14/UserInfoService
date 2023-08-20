namespace UserInfoService.Core.Exceptions
{
    public class InValidRequestDataException : Exception
    {
        public  int StatusCode { get; set; }

        public InValidRequestDataException() : base() { }

        public InValidRequestDataException(string message) 
            : base(message) { }

        public InValidRequestDataException(string message, Exception innerException) 
            : base(message, innerException) { }

        public InValidRequestDataException(string message, int statusCode)
           : this(message)
        { 
            StatusCode = statusCode;
        }

    }
}
