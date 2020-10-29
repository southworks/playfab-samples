using TicTacToeFunctions.Models.Exceptions;
using TicTacToeFunctions.Models.Responses;

namespace TicTacToeFunctions.Util
{
    public static class TicTacToeExceptionUtil
    {
        public static TicTacToeException LobbyFullException(string message = null)
        {
            return new TicTacToeException(StatusCode.LobbyFull, message);
        }

        public static TicTacToeException NotFoundException(string message = null)
        {
            return new TicTacToeException(StatusCode.NotFound, message);
        }

        public static TicTacToeException RequesterIsLobbyCreatorException(string message = null)
        {
            return new TicTacToeException(StatusCode.RequesterIsLobbyCreator, message);
        }

        public static TicTacToeException NotInvitationCodeIncludedException(string message = null)
        {
            return new TicTacToeException(StatusCode.NotInvitationCodeIncluded, message);
        }

        public static TicTacToeException InvalidInvitationCodeException(string message = null)
        {
            return new TicTacToeException(StatusCode.InvalidInvitationCode, message);
        }

        public static TicTacToeException OnlyLobbyCreatorCanLockException(string message = null)
        {
            return new TicTacToeException(StatusCode.OnlyLobbyCreatorCanLock, message);
        }

        public static ResponseWrapper<T> GetEmptyResponseWrapperFromException<T>(TicTacToeException exception)
        {
            return new ResponseWrapper<T> { StatusCode = exception.StatusCode, Message = exception.Message };
        }
    }
}
