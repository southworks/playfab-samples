using System;

namespace TicTacToeFunctions.Models.Exceptions
{
    public class TicTacToeException : Exception
    {
        public TicTacToeException() : base()
        {
            StatusCode = StatusCode.InternalServerError;
        }

        public TicTacToeException(string message) : base(message)
        {
            StatusCode = StatusCode.InternalServerError;
        }

        public TicTacToeException(string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = StatusCode.InternalServerError;
        }

        public TicTacToeException(StatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public TicTacToeException(StatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public StatusCode StatusCode { get; }
    }
}
