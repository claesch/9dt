using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace _9dt.Exceptions
{
    public class BaseException : HttpResponseException
    {
        public BaseException(HttpStatusCode statusCode, string message) : base(new HttpResponseMessage(statusCode) { Content = new StringContent(message) })
        { }
    }
}