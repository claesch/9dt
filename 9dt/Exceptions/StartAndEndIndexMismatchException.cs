using System.Net;

namespace _9dt.Exceptions
{
    public class StartAndEndIndexMismatchException : BaseException
    {
        public StartAndEndIndexMismatchException() : base(HttpStatusCode.BadRequest, "The index of start cannot be greater than end")
        {
        }
    }
}