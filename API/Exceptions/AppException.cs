using System;

namespace API.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}