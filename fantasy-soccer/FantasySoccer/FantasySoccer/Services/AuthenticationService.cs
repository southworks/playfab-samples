using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Models.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayFab;
using PlayFab.ClientModels;

namespace FantasySoccer.Services
{
    public class AuthenticationService: IAuthenticationService
    {
        private readonly PlayFabConfiguration config;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<AuthenticationService> logger;

        public AuthenticationService(
            IOptions<PlayFabConfiguration> options,
            ILogger<AuthenticationService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            config = options.Value;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;

            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = config.TitleId;
            }
        }

        #region Authentication

        public async Task<AuthResponse> LoginWithEmailAsync(string email, string password)
        {
            var req = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetUserAccountInfo = true,
                    GetUserVirtualCurrency = true
                }
            };

            try
            {
                var res = await PlayFabClientAPI.LoginWithEmailAddressAsync(req);

                if (res.Error != null)
                {
                    return await GetAuthResponseAsync(tokenExpiration: DateTime.UtcNow, errorMessage: res.Error.ErrorMessage);
                }

                var playFabResult = new PlayFabResult
                {
                    DisplayName = res.Result?.InfoResultPayload?.AccountInfo?.Username,
                    Email = email,
                    SessionTicket = res.Result?.SessionTicket,
                    PlayFabId = res.Result?.PlayFabId,
                    Budget = res.Result?.InfoResultPayload?.UserVirtualCurrency?[config.Currency] ?? 0
                };

                return await GetAuthResponseAsync((DateTime)res.Result.EntityToken.TokenExpiration, playFabResult);
            }
            catch (Exception exception)
            {
                HandleError(exception);
                return await GetAuthResponseAsync(tokenExpiration: DateTime.UtcNow, errorMessage: exception.Message);
            }
        }

        public async Task<AuthResponse> RegisterWithEmailAsync(string email, string password, string displayName = "")
        {
            var display = !string.IsNullOrWhiteSpace(displayName) ? displayName : email.Substring(0, Math.Min(email.Length, 24));

            var req = new RegisterPlayFabUserRequest
            {
                RequireBothUsernameAndEmail = false,
                Email = email,
                Password = password,
                DisplayName = display,
            };

            try
            {
                var res = await PlayFabClientAPI.RegisterPlayFabUserAsync(req);

                if (res.Error != null)
                {
                    return await GetAuthResponseAsync(tokenExpiration: DateTime.UtcNow, errorMessage: res.Error.ErrorMessage);
                }

                var playFabResult = new PlayFabResult
                {
                    DisplayName = res.Result?.Username,
                    Email = email,
                    SessionTicket = res.Result?.SessionTicket,
                    PlayFabId = res.Result?.PlayFabId,
                    Budget = FantasySoccerConstants.MaxBudget
                };

                return await GetAuthResponseAsync((DateTime)res.Result.EntityToken.TokenExpiration, playFabResult, true);
            }
            catch (Exception exception)
            {
                HandleError(exception);
                return await GetAuthResponseAsync(tokenExpiration: DateTime.UtcNow, errorMessage: exception.Message);
            }
        }

        public async Task<AuthResponse> LoginWithOpenIDConnectAsync(string idToken, ClaimsIdentity identity, AuthenticationProperties authenticationProperties, bool createAccount = true)
        {
            var request = new LoginWithOpenIdConnectRequest
            {
                ConnectionId = config.ConnectionId,
                IdToken = idToken,
                CreateAccount = createAccount,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetUserAccountInfo = true,
                    GetUserVirtualCurrency = true
                }
            };

            try
            {
                var response = await PlayFabClientAPI.LoginWithOpenIdConnectAsync(request);

                if (response.Error != null)
                {
                    return await GetAuthResponseAsync(tokenExpiration: DateTime.UtcNow, errorMessage: response.Error.ErrorMessage);
                }

                var playFabResult = new PlayFabResult
                {
                    DisplayName = response.Result?.InfoResultPayload?.AccountInfo?.Username,
                    SessionTicket = response.Result?.SessionTicket,
                    PlayFabId = response.Result?.PlayFabId,
                    Budget = response.Result?.InfoResultPayload?.UserVirtualCurrency?[config.Currency] ?? 0
                };

                return await GetAuthResponseAsync((DateTime)response.Result.EntityToken.TokenExpiration, playFabResult, (bool)response.Result?.NewlyCreated, identity: identity, authenticationProperties: authenticationProperties);
            }
            catch (Exception exception)
            {
                HandleError(exception);
                return await GetAuthResponseAsync(tokenExpiration: DateTime.UtcNow, errorMessage: exception.Message);
            }
        }

        public async Task LogoutAsync()
        {
            await httpContextAccessor.HttpContext.SignOutAsync();
        }

        #endregion

        private async Task SetSessionAsync(PlayFabSession session, ClaimsIdentity identity = null, AuthenticationProperties authenticationProperties = null)
        {
            var claimsIdentity = identity ?? new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(Helpers.Constants.Authentication.SchemaClaim, Helpers.Constants.Authentication.PlayFabSchema)
                },
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "User"));

            if (!string.IsNullOrEmpty(session.Email))
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, session.Email));
            }

            claimsIdentity.AddClaim(new Claim(PlayFabClaims.Budget, $"{config.Currency} {session.Budget}"));

            if (!string.IsNullOrEmpty(session.PlayFabId))
            {
                claimsIdentity.AddClaim(new Claim(PlayFabClaims.PlayFabId, session.PlayFabId));
            }

            var authProperties = authenticationProperties ?? new AuthenticationProperties();

            authProperties.IsPersistent = true;

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

            await httpContextAccessor.HttpContext.SignOutAsync();

            await httpContextAccessor.HttpContext.SignInAsync(
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private async Task<AuthResponse> GetAuthResponseAsync(
            DateTime tokenExpiration,
            PlayFabResult playFabResult = null,
            bool newlyCreated = false,
            string errorMessage = null,
            ClaimsIdentity identity = null,
            AuthenticationProperties authenticationProperties = null)
        {
            var localTokenExpiration = TimeZoneInfo.ConvertTimeFromUtc(tokenExpiration, TimeZoneInfo.Local);

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                var playFabSession = new PlayFabSession
                {
                    SessionTicket = playFabResult.SessionTicket,
                    Email = playFabResult.Email,
                    PlayFabId = playFabResult.PlayFabId,
                    Budget = playFabResult.Budget,
                    TokenExpiration = localTokenExpiration.ToString()
                };

                await SetSessionAsync(playFabSession, identity, authenticationProperties);
            }

            return new AuthResponse
            {
                ErrorMessage = errorMessage,
                Email = playFabResult?.Email,
                PlayFabId = playFabResult?.PlayFabId,
                UserName = playFabResult?.DisplayName,
                NewlyCreated = newlyCreated,
            };
        }

        private void HandleError(Exception exception)
        {
            if (exception.Source == "PlayFabAllSDK")
            {
                logger.LogError($"PlayFabAllSDK ERROR: API call failed with the message '{exception.Message}'.");
            }
            else
            {
                logger.LogError($"{exception.Message}");
            }
        }
    }
}
