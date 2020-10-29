using PlayFab.AuthenticationModels;

namespace TicTacToe.Models
{
    public class PlayerInfo
    {
        public string EntityToken { get; set; }

        public string PlayFabId { get; set; }

        public string SessionTicket { get; set; }

        public EntityKey Entity { get; set; }
    }
}
