namespace TicTacToeFunctions.Models.Game.Requests
{
    public class SetGameWinnerRequest
    {
        public string SharedGroupId { get; set; }

        public string PlayerId { get; set; }
    }
}