using System.Net;

namespace _9dt.Exceptions
{
    public class GameNotFoundException : BaseException
    {
        public GameNotFoundException() : base(HttpStatusCode.NotFound, "The game requested could not be found")
        {
        }
    }
}
