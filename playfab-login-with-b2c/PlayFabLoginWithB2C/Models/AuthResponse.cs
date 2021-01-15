using PlayFab.ClientModels;

namespace PlayFabLoginWithB2C.Models
{
    public class AuthResponse
    {
        public string ErrorMessage { get; set; }
        public UserAccountInfo AccountInfo { get; set; }
        public bool NewlyCreated { get; set; }
    }
}