using System.Net;

namespace _9dt.Exceptions
{
    public class PlayerMovedOutOfTurnException : BaseException
    {
        public PlayerMovedOutOfTurnException() : base(HttpStatusCode.Conflict, "Player moved out of turn")
        {
        }
    }
}