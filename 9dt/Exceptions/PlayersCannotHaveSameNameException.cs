using System.Net;

namespace _9dt.Exceptions
{
    public class PlayersCannotHaveSameNameException : BaseException
    {
        public PlayersCannotHaveSameNameException() : base(HttpStatusCode.BadRequest, "Unique player names are required")
        {
        }
    }
}