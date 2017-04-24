using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class PlayerMovedOutOfTurnException : HttpResponseException
    {
        public PlayerMovedOutOfTurnException() : base(HttpStatusCode.Conflict)
        {
        }
    }
}