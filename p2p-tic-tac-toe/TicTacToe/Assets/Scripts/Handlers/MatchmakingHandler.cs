using System.Collections;
using PlayFab;
using PlayFab.MultiplayerModels;
using TicTacToe.Helpers;
using TicTacToe.Models;
using TicTacToe.Models.Helpers;
using UnityEngine;

namespace TicTacToe.Handlers
{
    public class MatchmakingHandler : AzureFunctionRequestHandler, IMatchmakingHandler, IMatchmakingQueueConfiguration
    {
        public MatchmakingQueueConfiguration QueueConfiguration { get; set; }

        public CreateMatchmakingTicketResult MatchmakingTicketResult { get; set; }

        public JoinMatchmakingTicketResult JoinMatchmakingTicketResult { get; set; }

        public GetMatchmakingTicketResult MatchmakingTicketStatus { get; set; }

        public GetMatchResult MatchResult { get; set; }

        public MatchmakingHandler(PlayerInfo player) : base(player)
        {
            QueueConfiguration = new QuickMatchQueueConfiguration(this);
        }

        public IEnumerator CreateTicket(string attribute, string networkId = "")
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.CreateMatchmakingTicket(
                GetCreateRequest(GetPlayerAttribute(attribute, networkId)),
                (result) =>
                {
                    ExecutionCompleted = true;
                    MatchmakingTicketResult = result;
                },
                (error) =>
                {
                    var result = $"On CreateMatchmakingTicket failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator GetTicketStatus()
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest
                {
                    EscapeObject = QueueConfiguration.EscapeObject,
                    QueueName = QueueConfiguration.QueueName,
                    TicketId = MatchmakingTicketResult.TicketId,
                    AuthenticationContext = PlayFabAuthenticationContext
                },
                (result) =>
                {
                    ExecutionCompleted = true;
                    MatchmakingTicketStatus = result;
                },
                (error) =>
                {
                    var result = $"On GetTicketStatus failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator CancelPlayerTickets()
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(
                new CancelAllMatchmakingTicketsForPlayerRequest
                {
                    QueueName = QueueConfiguration.QueueName,
                    Entity = new EntityKey
                    {
                        Id = Player.Entity.Id,
                        Type = Player.Entity.Type,
                    },
                    AuthenticationContext = PlayFabAuthenticationContext
                },
                (result) =>
                {
                    ExecutionCompleted = true;
                    // set both objects to null as the related ticket was cancelled
                    MatchmakingTicketResult = null;
                    MatchmakingTicketStatus = null;
                },
                (error) =>
                {
                    var result = $"On CancelPlayerTickets failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator GetMatch()
        {
            ExecutionCompleted = false;

            PlayFabMultiplayerAPI.GetMatch(
                new GetMatchRequest
                {
                    EscapeObject = QueueConfiguration.EscapeObject,
                    MatchId = MatchmakingTicketStatus.MatchId,
                    QueueName = QueueConfiguration.QueueName,
                    ReturnMemberAttributes = QueueConfiguration.ReturnMemberAttributes,
                    AuthenticationContext = PlayFabAuthenticationContext,
                },
                (result) =>
                {
                    ExecutionCompleted = true;
                    MatchResult = result;
                },
                (error) =>
                {
                    var result = $"On GetMatch failed. Message: {error.ErrorMessage}, Code: {error.HttpCode}";
                    Debug.Log(result);
                }
            );

            yield return WaitForExecution();
        }

        public IEnumerator EnsureGetTicketStatus()
        {
            do
            {
                yield return GetTicketStatus();

                if (MatchmakingTicketStatus.Status == MatchmakingTicketStatusEnum.Canceled)
                {
                    MatchmakingTicketStatus = null;
                }

                if (MatchmakingTicketStatus != null && MatchmakingTicketStatus.Status != MatchmakingTicketStatusEnum.Matched)
                {
                    yield return new WaitForSeconds(Constants.RETRY_GET_TICKET_STATUS_AFTER_SECONDS);
                }
            }
            while (MatchmakingTicketStatus != null
                && MatchmakingTicketStatus.Status != MatchmakingTicketStatusEnum.Matched
                && MatchmakingTicketStatus.Status != MatchmakingTicketStatusEnum.Canceled);

            yield return WaitForExecution();
        }

        public void ChangeQueueConfiguration(QueueTypes queueType)
        {
            QueueConfiguration.ChangeQueueConfiguration(queueType);
        }

        private CreateMatchmakingTicketRequest GetCreateRequest(MatchmakingPlayerAttributes attributes)
        {
            var request = new CreateMatchmakingTicketRequest
            {
                Creator = MatchmakingHelper.GetMatchmakingPlayer(Player, attributes),
                GiveUpAfterSeconds = QueueConfiguration.GiveUpAfterSeconds,
                QueueName = QueueConfiguration.QueueName,
                AuthenticationContext = PlayFabAuthenticationContext
            };

            return request;
        }

        private MatchmakingPlayerAttributes GetPlayerAttribute(string attribute, string networkId)
        {
            return QueueConfiguration.GetMatchmakingPlayerAttributes(attribute, networkId);
        }
    }
}