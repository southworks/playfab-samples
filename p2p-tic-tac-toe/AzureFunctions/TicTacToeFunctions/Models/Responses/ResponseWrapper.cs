using TicTacToeFunctions.Models.Exceptions;

namespace TicTacToeFunctions.Models.Responses
{
    public class ResponseWrapper<T>
    {
        public StatusCode StatusCode { get; set; }

        public string Message { get; set; }

        public T Response { get; set; }
    }
}
