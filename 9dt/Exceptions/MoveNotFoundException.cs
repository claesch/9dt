using System.Net;

namespace _9dt.Exceptions
{
    public class MoveNotFoundException : BaseException
    {
        public MoveNotFoundException() : base(HttpStatusCode.NotFound, "The move(s) requested could not be found")
        {
        }
    }
}