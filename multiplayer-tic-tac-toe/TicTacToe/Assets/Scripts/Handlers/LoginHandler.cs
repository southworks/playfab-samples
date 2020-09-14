// Copyright (C) Microsoft Corporation. All rights reserved.

using PlayFab;
using PlayFab.ClientModels;
using System;
using TicTacToe.Models;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class LoginHandler
    {
        public void Login(Action<PlayerInfo> OnSuccess, Action OnError)
        {
            Settings.UpdateSettings();

            var request = new LoginWithCustomIDRequest
            {
                CustomId = this.GetPlayerCustomId(),
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(request,
                (result) =>
                {
                    var currentPlayer = new PlayerInfo
                    {
                        EntityToken = result.EntityToken.EntityToken,
                        PlayFabId = result.PlayFabId,
                        SessionTicket = result.SessionTicket,
                        Entity = new PlayFab.AuthenticationModels.EntityKey
                        {
                            Id = result.EntityToken.Entity.Id,
                            Type = result.EntityToken.Entity.Type
                        }
                    };

                    OnSuccess(currentPlayer);
                },
                (error) =>
                {
                    // Provide error feedback to user in case of failure at login
                    Debug.LogError("Could not login to PlayFab for Player.");
                    Debug.LogError($"Response code: {error.HttpCode}");
                    Debug.LogError($"Error details: {error.ErrorDetails}");
                    Debug.LogError($"Error message: {error.ErrorMessage}");

                    OnError();
                });
        }

        private string GetPlayerCustomId()
        {
            if (!PlayerPrefs.HasKey(Constants.PLAYFAB_PLAYER_CUSTOM_ID))
            {
                var newId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                PlayerPrefs.SetString(Constants.PLAYFAB_PLAYER_CUSTOM_ID, newId);
            }
            return PlayerPrefs.GetString(Constants.PLAYFAB_PLAYER_CUSTOM_ID);
        }
    }


}
