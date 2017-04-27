using System.Net;

namespace _9dt.Exceptions
{
    public class NumberOfPlayersMustBeTwoException : BaseException
    {
        public NumberOfPlayersMustBeTwoException() : base(HttpStatusCode.BadRequest, "This game can only be played by two players")
        {
        }
    }
}