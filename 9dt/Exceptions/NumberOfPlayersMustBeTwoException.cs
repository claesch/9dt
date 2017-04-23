using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class NumberOfPlayersMustBeTwoException : HttpResponseException
    {
            public NumberOfPlayersMustBeTwoException() : base(HttpStatusCode.BadRequest)
        {
        }
    }
}