using System.Net;

namespace _9dt.Exceptions
{
    public class PlayerNameException : BaseException
    {
        public PlayerNameException() : base(HttpStatusCode.BadRequest, "Player names cannot match and must be at least one character long")
        {
        }
    }
}