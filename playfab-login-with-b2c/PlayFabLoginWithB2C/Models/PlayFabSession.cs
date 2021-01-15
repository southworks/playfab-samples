namespace PlayFabLoginWithB2C.Models
{
    internal class PlayFabSession
    {
        public string SessionTicket { get; set; }
        public string Email { get; set; }
        public string PlayFabId { get; set; }
        public string TokenExpiration { get; set; }
    }
}