using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayFab;
using PlayFab.ClientModels;
using PlayFabLoginWithB2C.Helpers;
using PlayFabLoginWithB2C.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlayFabLoginWithB2C.Services
{
    public class PlayFabService : IPlayFabService
    {
        private readonly PlayFabOptions _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PlayFabService> _logger;

        public PlayFabService(IOptions<PlayFabOptions> options, ILogger<PlayFabService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _config = options.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = _config.TitleId;
            }
        }

        #region Authentication
        public async Task<AuthResponse> LoginWithEmail(string email, string password)
        {
            var req = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetUserAccountInfo = true
                }
            };

            try
            {
                var res = await PlayFabClientAPI.LoginWithEmailAddressAsync(req);

                if (res.Error != null)
                {
                    return GetAuthResponse(tokenExpiration: DateTime.UtcNow, errorMessage: res.Error.ErrorMessage);
                }

                return GetAuthResponse((DateTime)res.Result.EntityToken.TokenExpiration, email, res.Result?.SessionTicket);
            }
            catch (Exception exception)
            {
                HandleError(exception);
                return GetAuthResponse(tokenExpiration: DateTime.UtcNow, errorMessage: exception.Message);
            }
        }

        public async Task<AuthResponse> RegisterWithEmail(string email, string password)
        {
            var req = new RegisterPlayFabUserRequest
            {
                RequireBothUsernameAndEmail = false,
                Email = email,
                Password = password,
                DisplayName = email
            };

            try
            {
                var res = await PlayFabClientAPI.RegisterPlayFabUserAsync(req);

                if (res.Error != null)
                {
                    return GetAuthResponse(tokenExpiration: DateTime.UtcNow, errorMessage: res.Error.ErrorMessage);
                }

                return GetAuthResponse((DateTime)res.Result.EntityToken.TokenExpiration, email, res.Result?.SessionTicket);
            }
            catch (Exception exception)
            {
                HandleError(exception);
                return GetAuthResponse(tokenExpiration: DateTime.UtcNow, errorMessage: exception.Message);
            }
        }

        public async void Logout()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync();
        }
        #endregion

        public async Task<GetAccountInfoResponse> GetAccountInfo()
        {
            var sessionTicket = await GetSessionTicket();

            if (string.IsNullOrWhiteSpace(sessionTicket))
            {
                throw new Exception(Constants.ACCESS_DENIED_ERROR);
            }

            var req = new GetAccountInfoRequest
            {
                AuthenticationContext = new PlayFabAuthenticationContext { ClientSessionTicket = sessionTicket }
            };

            try
            {
                var res = await PlayFabClientAPI.GetAccountInfoAsync(req);

                if (res.Error != null)
                {
                    throw new Exception(res.Error.ErrorMessage);
                }

                return new GetAccountInfoResponse
                {
                    LinkedAccounts = GetLinkedAcccounts(res.Result.AccountInfo),
                    PlayFabEmail = res.Result.AccountInfo.PrivateInfo.Email,
                };
            }
            catch (Exception exception)
            {
                HandleError(exception);
                throw;
            }
        }

        public async Task<AuthResponse> LoginWithOpenIDConnect(string idToken, bool createAccount = true)
        {
            var request = new LoginWithOpenIdConnectRequest
            {
                TitleId = _config.TitleId,
                ConnectionId = _config.ConnectionId,
                IdToken = idToken,
                CreateAccount = createAccount,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetUserAccountInfo = true
                }
            };

            try
            {
                var response = await PlayFabClientAPI.LoginWithOpenIdConnectAsync(request);

                if (response.Error != null)
                {
                    return GetAuthResponse(tokenExpiration: DateTime.UtcNow, errorMessage: response.Error.ErrorMessage);
                }

                return GetAuthResponse((DateTime)response.Result.EntityToken.TokenExpiration, null, response.Result?.SessionTicket, null, response.Result);
            }
            catch (Exception exception)
            {
                HandleError(exception);
                return GetAuthResponse(tokenExpiration: DateTime.UtcNow, errorMessage: exception.Message);
            }
        }

        public async Task UpdateUserDataWithClaims(IEnumerable<Claim> claims)
        {
            var claimsDictionary = new Dictionary<string, string>();
            var keysToUpdate = PlayFabClaims.Claims;
            var claimsToUpdate = claims.Where(c => keysToUpdate.Contains(c.Type));
            
            foreach (var claim in claimsToUpdate)
            {
                claimsDictionary.Add(claim.Type, claim.Value);
            }

            await UpdateUserData(claimsDictionary);
        }

        public async Task<Dictionary<string, string>> GetUserDataWithClaims(string playFabId)
        {
            var data = new Dictionary<string, string>();
            var result = await GetUserData(playFabId, PlayFabClaims.Claims);
            
            foreach (var item in result.Data)
            {
                data.Add(item.Key, item.Value.Value);
            }

            return data;
        }

        public async Task<UpdateUserDataResult> UpdateUserData(Dictionary<string, string> data)
        {
            var sessionTicket = await GetSessionTicket();

            if (string.IsNullOrWhiteSpace(sessionTicket))
            {
                throw new Exception(Constants.ACCESS_DENIED_ERROR);
            }

            var request = new UpdateUserDataRequest
            {
                AuthenticationContext = new PlayFabAuthenticationContext { ClientSessionTicket = sessionTicket},
                Data = data,
                Permission = UserDataPermission.Private
            };

            try
            {
                var res = await PlayFabClientAPI.UpdateUserDataAsync(request);
            
                if (res.Error != null)
                {
                    throw new Exception(res.Error.ErrorMessage);
                }

                return res.Result;
            }
            catch (Exception exception)
            {
                HandleError(exception);
                throw;
            }
        }

        public async Task<GetUserDataResult> GetUserData(string playFabId, List<string> keys)
        {
            var sessionTicket = await GetSessionTicket();

            if (string.IsNullOrWhiteSpace(sessionTicket))
            {
                throw new Exception(Constants.ACCESS_DENIED_ERROR);
            }

            var request = new GetUserDataRequest
            {
                AuthenticationContext = new PlayFabAuthenticationContext { ClientSessionTicket = sessionTicket },
                PlayFabId = playFabId,
                Keys = keys
            };

            try
            {
                var res = await PlayFabClientAPI.GetUserDataAsync(request);

                if (res.Error != null)
                {
                    throw new Exception(res.Error.ErrorMessage);
                }

                return res.Result;
            }
            catch (Exception exception)
            {
                HandleError(exception);
                throw;
            }
        }

        public async Task<string> GetSessionTicket()
        {
            var tokenExpiration = await _httpContextAccessor.HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "TokenExpiration");
            
            if (string.IsNullOrWhiteSpace(tokenExpiration))
            {
                return null;
            }

            if (DateTime.Compare(DateTime.Now, DateTime.Parse(tokenExpiration)) > 0)
            {
                Logout();
                return null;
            }

            return await _httpContextAccessor.HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "SessionTicket");
        }

        private async void SetSession(PlayFabSession session)
        {
            var claimsIdentity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "User"),
                },
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            if (!string.IsNullOrEmpty(session.Email))
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, session.Email));
            }

            if (!string.IsNullOrEmpty(session.PlayFabId))
            {
                claimsIdentity.AddClaim(new Claim(PlayFabClaims.PlayFabId, session.PlayFabId));
            }

            claimsIdentity.AddClaim(new Claim("SessionTicket", session.SessionTicket));

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
            };

            authProperties.StoreTokens(new List<AuthenticationToken> {
                new AuthenticationToken
                {
                    Name = "SessionTicket",
                    Value = session.SessionTicket
                },
                new AuthenticationToken
                {
                    Name = "TokenExpiration",
                    Value = session.TokenExpiration
                }
            });

            await _httpContextAccessor.HttpContext.SignOutAsync(AzureADB2CDefaults.AuthenticationScheme);

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private AuthResponse GetAuthResponse(DateTime tokenExpiration, string email = null, string sessionTicket = null, string errorMessage = null, LoginResult loginResult = null)
        {
            var localTokenExpiration = TimeZoneInfo.ConvertTimeFromUtc(tokenExpiration, TimeZoneInfo.Local);
            
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                var playFabSession = new PlayFabSession
                {
                    SessionTicket = sessionTicket,
                    Email = email,
                    PlayFabId = loginResult.InfoResultPayload.AccountInfo.PlayFabId,
                    TokenExpiration = localTokenExpiration.ToString()
                };
                SetSession(playFabSession);
            }

            return new AuthResponse
            {
                ErrorMessage = errorMessage,
                AccountInfo = loginResult.InfoResultPayload.AccountInfo,
                NewlyCreated = loginResult.NewlyCreated
            };
        }

        private void HandleError(Exception exception)
        {
            if (exception.Source == "PlayFabAllSDK")
            {
                _logger.LogError($"PlayFabAllSDK ERROR: API call failed with the message '{exception.Message}'.");
            }
            else
            {
                _logger.LogError($"{exception.Message}");
            }
        }

        private List<LinkedAccount> GetLinkedAcccounts(UserAccountInfo accountInfo)
        {
            var linkedAccounts = new List<LinkedAccount> { };

            linkedAccounts.Add(new LinkedAccount
            {
                Origin = "Google",
                ButtonClass = Constants.BUTTON_GOOGLE_CLASS,
                Linked = accountInfo.GoogleInfo != null,
                Name = accountInfo.GoogleInfo?.GoogleName,
                Email = accountInfo.GoogleInfo?.GoogleEmail
            });
            linkedAccounts.Add(new LinkedAccount
            {
                Origin = "Facebook",
                ButtonClass = Constants.BUTTON_FACEBOOK_CLASS,
                Linked = accountInfo.FacebookInfo != null,
                Name = accountInfo.FacebookInfo?.FullName
            });

            return linkedAccounts;
        }

    }
}
