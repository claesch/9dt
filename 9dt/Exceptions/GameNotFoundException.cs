using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class GameNotFoundException : HttpResponseException
    {
            public GameNotFoundException() : base(HttpStatusCode.NotFound)
        {
        }
    }
}