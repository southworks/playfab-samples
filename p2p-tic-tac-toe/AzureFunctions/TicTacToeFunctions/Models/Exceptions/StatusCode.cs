namespace TicTacToeFunctions.Models.Exceptions
{
    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        LobbyFull = 512,
        RequesterIsLobbyCreator = 513,
        NotInvitationCodeIncluded = 432,
        InvalidInvitationCode = 514,
        OnlyLobbyCreatorCanLock = 515
    }
}
