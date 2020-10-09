using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static async Task<TicTacToeSharedGroupData> JoinMatchLobby(PlayFabAuthenticationContext authenticationContext, string matchLobbyId, string playerTwo, DocumentClient documentClient)
        {
            var tttShareGroupData = await SharedGroupDataUtil.GetAsync(authenticationContext, matchLobbyId);

            // we need the document reference for the Cosmos DB for using its "etag",
            // which avoids having race-conditions when writing in DDBB.
            var documentLobby = await GetMatchLobbyFromClient(documentClient, matchLobbyId);

            if (tttShareGroupData?.Match == null)
            {
                throw new Exception("Match not exists");
            }

            if (documentLobby == null)
            {
                throw new Exception("Match Lobby does not exist in Database");
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
            tttShareGroupData.MatchLobby.CurrentAvailability--;

            // update the current availability in Cosmos DB
            documentLobby.CurrentAvailability = tttShareGroupData.MatchLobby.CurrentAvailability;

            // This method will update the Cosmos DB document with the newest availability.
            // If there was an outside-modification by other player (race condition),
            // this will throw an error and the player won't be able to join this Lobby.
            await DocumentClientManager.ReplaceDocument(documentClient, documentLobby, Constants.DATABASE_NAME, Constants.MATCH_LOBBY_TABLE_NAME);

            await SharedGroupDataUtil.AddMembersAsync(
                authenticationContext,
                matchLobbyId,
                new List<string> {
                    tttShareGroupData.Match.PlayerTwoId
                }
            );

            await SharedGroupDataUtil.UpdateAsync(authenticationContext, tttShareGroupData);

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

        private static async Task<MatchLobby> GetMatchLobbyFromClient(DocumentClient documentClient, string matchLobbyId)
        {
            var filteredLobbies = await GetMatchLobbyInfoAsync(documentClient, (matchLobby) => matchLobby.id == matchLobbyId);

            return filteredLobbies.First() ?? null;
        }
    }
}
