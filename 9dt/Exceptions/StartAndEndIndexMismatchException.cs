using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class StartAndEndIndexMismatchException : HttpResponseException
    {
        public StartAndEndIndexMismatchException() : base(HttpStatusCode.BadRequest)
        {
        }
    }
}