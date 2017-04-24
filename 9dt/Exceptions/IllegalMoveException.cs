using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class IllegalMoveException : HttpResponseException
    {
        public IllegalMoveException() : base(HttpStatusCode.Conflict)
        {
        }
    }
}