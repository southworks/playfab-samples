using PlayFab.ClientModels;
using PlayFabLoginWithB2C.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlayFabLoginWithB2C.Services
{
    public interface IPlayFabService
    {
        Task<AuthResponse> LoginWithEmail(string email, string password);
        Task<AuthResponse> RegisterWithEmail(string email, string password);
        Task<AuthResponse> LoginWithOpenIDConnect(string idToken, bool createAccount = true);
        Task<GetAccountInfoResponse> GetAccountInfo();
        Task UpdateUserDataWithClaims(IEnumerable<Claim> claims);
        Task<UpdateUserDataResult> UpdateUserData(Dictionary<string, string> data);
        Task<Dictionary<string, string>> GetUserDataWithClaims(string playFabId);
        Task<GetUserDataResult> GetUserData(string playFabId, List<string> keys);
        Task<string> GetSessionTicket();
        void Logout();

    }
}