using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class PlayerNotFoundException : HttpResponseException
    {
            public PlayerNotFoundException() : base(HttpStatusCode.NotFound)
        {
        }
    }
}