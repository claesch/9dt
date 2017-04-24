using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class MoveNotFoundException : HttpResponseException
    {
        public MoveNotFoundException() : base(HttpStatusCode.NotFound)
        {
        }
    }
}