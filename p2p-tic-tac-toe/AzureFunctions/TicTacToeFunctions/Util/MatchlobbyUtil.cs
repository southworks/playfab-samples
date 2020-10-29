using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using TicTacToeFunctions.Models;
using TicTacToeFunctions.Models.Responses;

namespace TicTacToeFunctions.Util
{
    public static class MatchLobbyUtil
    {
        private const string NetworkIdInviteSeparator = "|";

        public static async Task<MatchLobby> CreateMatchLobby(string matchLobbyId, bool lockedState, string creatorId, string networkId, IAsyncCollector<MatchLobby> matchlobbyCollection, DocumentClient documentClient)
        {
            var matchLobby = await GetMatchLobbyAsyncById(documentClient, matchLobbyId);

            if (matchLobby != null)
            {
                throw new Exception("Match already exists");
            }

            matchLobby = new MatchLobby
            {
                MatchLobbyId = matchLobbyId,
                CurrentAvailability = 1,
                NetworkId = networkId,
                CreatorId = creatorId,
                Locked = lockedState
            };

            await matchlobbyCollection.AddAsync(matchLobby);

            return matchLobby;
        }

        public static async Task<MatchLobby> JoinMatchLobby(string matchLobbyId, string playerTwo, DocumentClient documentClient, string invitationCode = null)
        {
            var matchLobby = await GetMatchLobbyAsyncById(documentClient, matchLobbyId);

            if (matchLobby == null)
            {
                throw TicTacToeExceptionUtil.NotFoundException(Constants.ExceptionLobbyNotFound);
            }

            if (matchLobby.CurrentAvailability == 0)
            {
                throw TicTacToeExceptionUtil.LobbyFullException(Constants.ExceptionLobbyIsFull);
            }

            if (matchLobby.CreatorId == playerTwo)
            {
                throw TicTacToeExceptionUtil.RequesterIsLobbyCreatorException(Constants.ExceptionRequesterIsCreator);
            }

            if (matchLobby.Locked)
            {
                if (string.IsNullOrEmpty(invitationCode))
                {
                    throw TicTacToeExceptionUtil.NotInvitationCodeIncludedException(Constants.ExceptionMissingInvitationCode);
                }

                if (GetInvitationIdFromNetworkId(matchLobby.NetworkId) != invitationCode)
                {
                    throw TicTacToeExceptionUtil.InvalidInvitationCodeException(Constants.ExceptionInvalidInvitationCode);
                }
            }

            matchLobby.CurrentAvailability--;

            // This method will update the Cosmos DB document with the newest availability.
            // If there was an outside-modification by other player (race condition),
            // this will throw an error and the player won't be able to join this Lobby.
            await DocumentClientManager.ReplaceDocument(documentClient, matchLobby, Constants.DatabaseName, Constants.MatchLobbyTableName);

            return matchLobby;
        }

        public static async Task<List<MatchLobby>> GetMatchLobbiesAsync(DocumentClient client, Expression<Func<MatchLobby, bool>> expression)
        {
            return await GetDocumentsAsync(client, expression);
        }

        public static async Task<List<MatchLobbyDTO>> GetMatchLobbiesDTOAsync(DocumentClient client, Expression<Func<MatchLobbyDTO, bool>> expression)
        {
            return await GetDocumentsAsync(client, expression);
        }

        public static async Task<MatchLobby> GetMatchLobbyAsyncById(DocumentClient client, string matchLobbyId)
        {
            return await GetMatchLobbyAsync(client, (document) => document.MatchLobbyId == matchLobbyId);
        }

        public static async Task<MatchLobby> GetMatchLobbyAsync(DocumentClient client, Expression<Func<MatchLobby, bool>> expression)
        {
            return (await GetMatchLobbiesAsync(client, expression)).FirstOrDefault();
        }

        public static async Task DeleteMatchLobbyFromDDBBAsync(DocumentClient client, string matchLobbyPartitionId, string matchLobbyId)
        {
            var docsToDelete = await DocumentClientManager.FilterDocumentClient(
                client,
                Constants.DatabaseName,
                Constants.MatchLobbyTableName,
                ExpressionUtils.GetDocumentByIdExpression(matchLobbyPartitionId));

            if (docsToDelete == null || docsToDelete.Count == 0)
            {
                return;
            }

            foreach (var doc in docsToDelete)
            {
                await DocumentClientManager.DeleteDocumentFromClientAsync(client, doc, matchLobbyId);
            }
        }

        public static async Task<MatchLobby> LeaveMatchLobby(MatchLobby matchLobby, DocumentClient documentClient)
        {
            matchLobby.CurrentAvailability++;

            // This method will update the Cosmos DB document with the newest availability.
            // If there was an outside-modification by other player (race condition),
            // this will throw an error and the player won't be able to join this Lobby.
            await DocumentClientManager.ReplaceDocument(documentClient, matchLobby, Constants.DatabaseName, Constants.MatchLobbyTableName);

            return matchLobby;
        }

        public static async Task<MatchLobby> SetMatchLobbyLockState(string matchLobbyId, bool locked, string creatorId, DocumentClient documentClient)
        {
            var matchLobby = await GetMatchLobbyAsyncById(documentClient, matchLobbyId);

            if (matchLobby == null)
            {
                throw TicTacToeExceptionUtil.NotFoundException(Constants.ExceptionLobbyNotFound);
            }

            if (matchLobby.CreatorId != creatorId)
            {
                throw TicTacToeExceptionUtil.OnlyLobbyCreatorCanLockException(Constants.ExceptionOnlyCreatorCanLockLobby);
            }

            if (matchLobby.Locked == locked)
            {
                return matchLobby;
            }

            matchLobby.Locked = locked;

            await DocumentClientManager.ReplaceDocument(documentClient, matchLobby, Constants.DatabaseName, Constants.MatchLobbyTableName);

            return matchLobby;
        }

        private static async Task<List<T>> GetDocumentsAsync<T>(DocumentClient client, Expression<Func<T, bool>> expression)
        {
            return await DocumentClientManager.FilterDocumentClient(
                client,
                Constants.DatabaseName,
                Constants.MatchLobbyTableName,
                expression);
        }

        private static string GetInvitationIdFromNetworkId(string networkId)
        {
            var indexOfSeperator = networkId.IndexOf(NetworkIdInviteSeparator);

            if (indexOfSeperator != -1)
            {
                return networkId.Substring(0, indexOfSeperator);
            }

            throw new Exception($"Not valid network identifier: Network identifier {networkId} doesn't contain the separator {NetworkIdInviteSeparator}");
        }
    }
}
