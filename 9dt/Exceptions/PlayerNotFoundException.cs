using System.Net;

namespace _9dt.Exceptions
{
    public class PlayerNotFoundException : BaseException
    {
            public PlayerNotFoundException() : base(HttpStatusCode.NotFound, "The requested player is not part of this game")
        {
        }
    }
}