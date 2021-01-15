using System.Security.Claims;
using System.Threading.Tasks;
using FantasySoccer.Models.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace FantasySoccer.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> LoginWithEmailAsync(string email, string password);
        Task<AuthResponse> RegisterWithEmailAsync(string email, string password, string displayName = "");
        Task<AuthResponse> LoginWithOpenIDConnectAsync(string idToken, ClaimsIdentity identity, AuthenticationProperties authenticationProperties, bool createAccount = true);
        Task LogoutAsync();
    }
}
