namespace FantasySoccer.Models.Authentication
{
    internal class PlayFabSession
    {
        public string SessionTicket { get; set; }
        public string Email { get; set; }
        public string PlayFabId { get; set; }
        public string TokenExpiration { get; set; }
        public int Budget { get; set; }
    }
}
