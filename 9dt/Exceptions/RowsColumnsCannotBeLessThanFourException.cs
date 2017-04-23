using System.Net;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class RowsColumnsCannotBeLessThanFourException : HttpResponseException
    {
            public RowsColumnsCannotBeLessThanFourException() : base(HttpStatusCode.BadRequest)
        {
        }
    }
}