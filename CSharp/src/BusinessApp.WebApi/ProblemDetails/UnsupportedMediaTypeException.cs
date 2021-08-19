namespace BusinessApp.WebApi.ProblemDetails
{
    public class UnsupportedMediaTypeException : StatusCodeException
    {
        public UnsupportedMediaTypeException(string message)
            : base(415, message)
        { }
    }
}
