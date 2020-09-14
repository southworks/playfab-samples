// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TicTacToeFunctions.Models;

namespace TicTacToeFunctions.Util
{
    public static class MatchLobbyUtil
    {
        public static async Task<TicTacToeSharedGroupData> CreateMatchLobby(PlayFabAuthenticationContext authenticationContext, string sharedGroupId, string playerOne, IAsyncCollector<MatchLobby> matchlobbyCollection)
        {
            var sharedGroupData = new TicTacToeSharedGroupData
            {
                SharedGroupId = sharedGroupId,
                Match = new Match
                {
                    PlayerOneId = playerOne
                },
                MatchLobby = new MatchLobby
                {
                    MatchLobbyId = sharedGroupId,
                    CurrentAvailability = 1
                }
            };

            var sharedGroupDataUpdated = await SharedGroupDataUtil.UpdateAsync(authenticationContext, sharedGroupData);
            await matchlobbyCollection.AddAsync(sharedGroupDataUpdated.MatchLobby);

            return sharedGroupDataUpdated;
        }

        public static async Task<TicTacToeSharedGroupData> JoinMatchLobby(PlayFabAuthenticationContext authenticationContext, string matchLobbyId, string playerTwo, IAsyncCollector<MatchLobby> matchlobbyCollector)
        {
            var tttShareGroupData = await SharedGroupDataUtil.GetAsync(authenticationContext, matchLobbyId);

            if (tttShareGroupData?.Match == null)
            {
                throw new Exception("Match not exists");
            }

            if (!string.IsNullOrWhiteSpace(tttShareGroupData.Match.PlayerOneId) && !string.IsNullOrWhiteSpace(tttShareGroupData.Match.PlayerTwoId))
            {
                throw new Exception("Match is full");
            }

            if (string.Compare(tttShareGroupData.Match.PlayerOneId, playerTwo, true) == 0)
            {
                throw new Exception("This player is already the player one");
            }

            tttShareGroupData.Match.PlayerTwoId = playerTwo;

            await SharedGroupDataUtil.AddMembersAsync(
                authenticationContext,
                matchLobbyId,
                new List<string> {
                    tttShareGroupData.Match.PlayerTwoId
                }
            );

            tttShareGroupData.MatchLobby.CurrentAvailability--;
            await SharedGroupDataUtil.UpdateAsync(authenticationContext, tttShareGroupData);

            await matchlobbyCollector.AddAsync(tttShareGroupData.MatchLobby);

            return tttShareGroupData;
        }

        public static async Task<EmptyResponse> AddMember(PlayFabAuthenticationContext authenticationContext, string matchlobbyId, EntityKey member)
        {
            var apiSettings = new PlayFabApiSettings
            {
                TitleId = Environment.GetEnvironmentVariable(Constants.PLAYFAB_TITLE_ID, EnvironmentVariableTarget.Process),
                DeveloperSecretKey = Environment.GetEnvironmentVariable(Constants.PLAYFAB_DEV_SECRET_KEY, EnvironmentVariableTarget.Process)
            };
            var groupApi = new PlayFabGroupsInstanceAPI(apiSettings, authenticationContext);
            var members = new List<EntityKey>() { member };

            var response = await groupApi.AddMembersAsync(new AddMembersRequest
            {
                Group = new EntityKey
                {
                    Id = matchlobbyId,
                    Type = "group"
                },
                Members = members
            });

            if (response.Error != null)
            {
                throw new Exception($"An error occurred while the addition of member to the group: Error: {response.Error.GenerateErrorReport()}");
            }

            return response.Result;
        }

        public static async Task<List<MatchLobby>> GetMatchLobbyInfoAsync(DocumentClient client, Expression<Func<MatchLobby, bool>> expression)
        {
            return await DocumentClientManager.FilterDocumentClient(
                client,
                Constants.DATABASE_NAME,
                Constants.MATCH_LOBBY_TABLE_NAME,
                expression
            );
        }

        public static async Task DeleteMatchLobbyFromDDBBAsync(DocumentClient client, string matchLobbyPartitionId, string matchLobbyId)
        {
            var docsToDelete = await DocumentClientManager.FilterDocumentClient
            (
                client,
                Constants.DATABASE_NAME,
                Constants.MATCH_LOBBY_TABLE_NAME,
                ExpressionUtils.GetDeleteDocumentByIdExpression(matchLobbyPartitionId)
            );

            if (docsToDelete == null || docsToDelete.Count == 0)
            {
                return;
            }

            foreach (var doc in docsToDelete)
            {
                await DocumentClientManager.DeleteDocumentFromClientAsync(client, doc, matchLobbyId);
            }
        }

        public static async Task DeletePlayFabMatchLobbyAsync(PlayFabApiSettings apiSettings, PlayFabAuthenticationContext authenticationContext, string matchlobbyId)
        {
            var groupApi = new PlayFabGroupsInstanceAPI(apiSettings, authenticationContext);
            var request = new DeleteGroupRequest { Group = CreateEntityKey(matchlobbyId) };

            await groupApi.DeleteGroupAsync(request);
        }

        public static EntityKey CreateEntityKey(string id, string type = "")
        {
            return new EntityKey
            {
                Id = id,
                Type = type
            };
        }
    }
}
