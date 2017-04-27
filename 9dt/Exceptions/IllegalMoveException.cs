using System.Net;

namespace _9dt.Exceptions
{
    public class IllegalMoveException : BaseException
    {
        public IllegalMoveException(string message) : base(HttpStatusCode.BadRequest, $"Illegal move: {message}")
        {
        }
    }
}