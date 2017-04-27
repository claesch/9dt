using System.Net;

namespace _9dt.Exceptions
{
    public class RowsColumnsCannotBeLessThanFourException : BaseException
    {
        public RowsColumnsCannotBeLessThanFourException() : base(HttpStatusCode.BadRequest, "You must specify at least 4 rows and 4 columns")
        {
        }
    }
}