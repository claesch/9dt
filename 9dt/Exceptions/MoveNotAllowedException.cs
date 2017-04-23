using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class MoveNotAllowedException : HttpResponseException
    {
        public MoveNotAllowedException() : base(HttpStatusCode.Gone)
        {
        }
    }
}