using System.Net;

namespace _9dt.Exceptions
{
    public class MoveNotAllowedException : BaseException
    {
        public MoveNotAllowedException(string message) : base(HttpStatusCode.Gone, $"Move not allowed: {message}")
        {
        }
    }
}