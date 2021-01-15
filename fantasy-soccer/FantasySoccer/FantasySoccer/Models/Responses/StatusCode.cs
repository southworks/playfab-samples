namespace FantasySoccer.Models.Responses
{
    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        BadRequest = 400,
        NotFound = 404,
        UserTeamPlayerNotValid = 406,
        TournamentDataNotValid = 407,
    }
}
