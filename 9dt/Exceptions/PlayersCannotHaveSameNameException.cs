using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class PlayersCannotHaveSameNameException : HttpResponseException
    {
        public PlayersCannotHaveSameNameException() : base(HttpStatusCode.BadRequest)
        {
        }
    }
}