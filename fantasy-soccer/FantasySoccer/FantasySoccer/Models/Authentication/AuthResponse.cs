using PlayFab.ClientModels;

namespace FantasySoccer.Models.Authentication
{
    public class AuthResponse
    {
        public string ErrorMessage { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PlayFabId { get; set; }
        public bool NewlyCreated { get; set; }
    }
}
